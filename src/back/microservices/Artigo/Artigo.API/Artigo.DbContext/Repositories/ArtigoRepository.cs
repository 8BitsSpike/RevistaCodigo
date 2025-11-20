using Artigo.DbContext.Data;
using Artigo.DbContext.Interfaces;
using Artigo.DbContext.PersistenceModels;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Artigo.DbContext.Repositories
{
    public class ArtigoRepository : IArtigoRepository
    {
        private readonly IMongoCollection<ArtigoModel> _artigos;
        private readonly IMapper _mapper;

        public ArtigoRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _artigos = dbContext.Artigos;
            _mapper = mapper;
        }

        private IClientSessionHandle? GetSession(object? sessionHandle)
        {
            return (IClientSessionHandle?)sessionHandle;
        }

        // Projeção atualizada para incluir os novos campos
        private readonly ProjectionDefinition<ArtigoModel> _cardProjection =
            Builders<ArtigoModel>.Projection
                .Include(a => a.Id)
                .Include(a => a.Titulo)
                .Include(a => a.Resumo)
                .Include(a => a.DataCriacao)
                .Include(a => a.MidiaDestaque)
                .Include(a => a.Status)
                .Include(a => a.Tipo)
                .Include(a => a.PermitirComentario);

        // --- Implementação dos Métodos da Interface ---

        public async Task<Artigo.Intf.Entities.Artigo?> GetByIdAsync(string id, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _artigos.Find(session, a => a.Id == id)
                : _artigos.Find(a => a.Id == id);

            var model = await find.FirstOrDefaultAsync();
            return _mapper.Map<Artigo.Intf.Entities.Artigo>(model);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> GetByStatusAsync(StatusArtigo status, int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);
            var filter = Builders<ArtigoModel>.Filter.Eq(a => a.Status, status);

            var find = (session != null)
                ? _artigos.Find(session, filter)
                : _artigos.Find(filter);

            var models = await find
                .SortByDescending(a => a.DataCriacao)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListAsync(int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);
            var filter = Builders<ArtigoModel>.Filter.Eq(a => a.Status, StatusArtigo.Publicado);

            var find = (session != null)
                ? _artigos.Find(session, filter)
                : _artigos.Find(filter);

            var models = await find
                .Project<ArtigoModel>(_cardProjection)
                .SortByDescending(a => a.DataCriacao)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListPorTipoAsync(TipoArtigo tipo, int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            var filter = Builders<ArtigoModel>.Filter.Eq(a => a.Status, StatusArtigo.Publicado) &
                         Builders<ArtigoModel>.Filter.Eq(a => a.Tipo, tipo);

            var find = (session != null)
                ? _artigos.Find(session, filter)
                : _artigos.Find(filter);

            var models = await find
                .Project<ArtigoModel>(_cardProjection)
                .SortByDescending(a => a.DataCriacao)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var filter = Builders<ArtigoModel>.Filter.In(a => a.Id, ids);

            var find = (session != null)
                ? _artigos.Find(session, filter)
                : _artigos.Find(filter);

            var models = await find
                .SortByDescending(a => a.DataCriacao)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }

        public async Task AddAsync(Artigo.Intf.Entities.Artigo artigo, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var model = _mapper.Map<ArtigoModel>(artigo);

            if (session != null)
                await _artigos.InsertOneAsync(session, model);
            else
                await _artigos.InsertOneAsync(model);

            _mapper.Map(model, artigo);
        }

        public async Task<bool> UpdateAsync(Artigo.Intf.Entities.Artigo artigo, object? sessionHandle = null)
        {
            if (string.IsNullOrEmpty(artigo.Id)) return false;
            var session = GetSession(sessionHandle);
            var model = _mapper.Map<ArtigoModel>(artigo);

            var result = (session != null)
                ? await _artigos.ReplaceOneAsync(session, a => a.Id == artigo.Id, model)
                // Usando artigo.Id diretamente
                : await _artigos.ReplaceOneAsync(a => a.Id == artigo.Id, model);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> UpdateMetricsAsync(string id, int totalComentarios, int totalInteracoes, object? sessionHandle = null)
        {
            if (string.IsNullOrEmpty(id)) return false;
            var session = GetSession(sessionHandle);
            var filter = Builders<ArtigoModel>.Filter.Eq(a => a.Id, id);
            // Usando id diretamente

            var update = Builders<ArtigoModel>.Update
                .Set(a => a.TotalComentarios, totalComentarios)
                .Set(a => a.TotalInteracoes, totalInteracoes);

            var result = (session != null)
                ? await _artigos.UpdateOneAsync(session, filter, update)
                : await _artigos.UpdateOneAsync(filter, update);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteAsync(string id, object? sessionHandle = null)
        {
            if (string.IsNullOrEmpty(id)) return false;
            var session = GetSession(sessionHandle);

            var result = (session != null)
                ? await _artigos.DeleteOneAsync(session, a => a.Id == id)
                // Usando id diretamente
                : await _artigos.DeleteOneAsync(a => a.Id == id);

            return result.IsAcknowledged && result.DeletedCount == 1;
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosCardListByTitleAsync(string searchTerm, int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            var filter = Builders<ArtigoModel>.Filter.Eq(a => a.Status, StatusArtigo.Publicado);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                string escapedSearchTerm = System.Text.RegularExpressions.Regex.Escape(searchTerm);
                var searchFilter = Builders<ArtigoModel>.Filter.Regex(a => a.Titulo, new BsonRegularExpression($".*{escapedSearchTerm}.*", "i"));
                filter &= searchFilter;
            }

            var find = (session != null)
                ? _artigos.Find(session, filter)
                : _artigos.Find(filter);

            var models = await find
                .SortByDescending(a => a.DataCriacao)
                .Project<ArtigoModel>(_cardProjection)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosCardListByAutorIdsAsync(IReadOnlyList<string> autorIds, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            // 1. Filtro (Publicado E AutorId está na lista)
            var filter = Builders<ArtigoModel>.Filter.Eq(a => a.Status, StatusArtigo.Publicado) &
                         Builders<ArtigoModel>.Filter.AnyIn(a => a.AutorIds, autorIds);

            var find = (session != null)
                ? _artigos.Find(session, filter)
                : _artigos.Find(filter);

            var models = await find
                .SortByDescending(a => a.DataCriacao)
                .Project<ArtigoModel>(_cardProjection)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }

        /// <sumario>
        /// (STAFF) Busca artigos (formato card) de um único Autor.
        /// NÃO FILTRA POR STATUS - Retorna todos (Rascunho, Publicado, etc.)
        /// </sumario>
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListPorAutorIdAsync(string autorId, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            // 1. Filtro (AutorId está na lista AutorIds)
            // SEM FILTRO DE STATUS
            var filter = Builders<ArtigoModel>.Filter.AnyIn(a => a.AutorIds, new[] { autorId });

            var find = (session != null)
                ? _artigos.Find(session, filter)
                : _artigos.Find(filter);

            var models = await find
                .SortByDescending(a => a.DataCriacao)
                .Project<ArtigoModel>(_cardProjection) // Usa a projeção de card
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosCardListByAutorReferenceAsync(string searchTerm, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            string fullMatchRegex = $"^{System.Text.RegularExpressions.Regex.Escape(searchTerm)}$";

            // 1. Filtro (Publicado E Regex no AutorReference)
            var filter = Builders<ArtigoModel>.Filter.Eq(a => a.Status, StatusArtigo.Publicado) &
                         Builders<ArtigoModel>.Filter.Regex(a => a.AutorReference, new BsonRegularExpression(fullMatchRegex, "i"));

            var find = (session != null)
                ? _artigos.Find(session, filter)
                : _artigos.Find(filter);

            var models = await find
                .SortByDescending(a => a.DataCriacao)
                .Project<ArtigoModel>(_cardProjection)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }

        // --- (MÉTODOS PARA STAFF) ---

        /// <sumario>
        /// (STAFF) Retorna artigos (card) filtrados por TipoArtigo, SEM filtro de status.
        /// </sumario>
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosEditorialPorTipoAsync(TipoArtigo tipo, int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            // Apenas filtra por Tipo
            var filter = Builders<ArtigoModel>.Filter.Eq(a => a.Tipo, tipo);

            var find = (session != null)
                ? _artigos.Find(session, filter)
                : _artigos.Find(filter);

            var models = await find
                .Project<ArtigoModel>(_cardProjection)
                .SortByDescending(a => a.DataCriacao)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }

        /// <sumario>
        /// (STAFF) Busca artigos (card) por Título, SEM filtro de status.
        /// </sumario>
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosEditorialByTitleAsync(string searchTerm, int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            // Apenas filtra por Título
            string escapedSearchTerm = System.Text.RegularExpressions.Regex.Escape(searchTerm);
            var filter = Builders<ArtigoModel>.Filter.Regex(a => a.Titulo, new BsonRegularExpression($".*{escapedSearchTerm}.*", "i"));

            var find = (session != null)
                ? _artigos.Find(session, filter)
                : _artigos.Find(filter);

            var models = await find
                .SortByDescending(a => a.DataCriacao)
                .Project<ArtigoModel>(_cardProjection)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }

        /// <sumario>
        /// (STAFF) Busca artigos (card) por IDs de Autor, SEM filtro de status.
        /// </sumario>
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosEditorialByAutorIdsAsync(IReadOnlyList<string> autorIds, int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            var filter = Builders<ArtigoModel>.Filter.AnyIn(a => a.AutorIds, autorIds);

            var find = (session != null)
                ? _artigos.Find(session, filter)
                : _artigos.Find(filter);

            var models = await find
                .SortByDescending(a => a.DataCriacao)
                .Project<ArtigoModel>(_cardProjection)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }
    }
}