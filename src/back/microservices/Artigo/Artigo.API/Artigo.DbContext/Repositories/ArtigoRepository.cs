using Artigo.DbContext.Data;
using Artigo.DbContext.Interfaces;
using Artigo.DbContext.PersistenceModels;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
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

        // --- Implementação dos Métodos da Interface ---

        public async Task<Artigo.Intf.Entities.Artigo?> GetByIdAsync(string id, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;
            var session = GetSession(sessionHandle);

            // *** CORREÇÃO ***
            var find = (session != null)
                ? _artigos.Find(session, a => a.Id == objectId.ToString())
                : _artigos.Find(a => a.Id == objectId.ToString());

            var model = await find.FirstOrDefaultAsync();
            return _mapper.Map<Artigo.Intf.Entities.Artigo>(model);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> GetByStatusAsync(StatusArtigo status, int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);
            var filter = Builders<ArtigoModel>.Filter.Eq(a => a.Status, status);

            // *** CORREÇÃO ***
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

            var projection = Builders<ArtigoModel>.Projection
                .Include(a => a.Id)
                .Include(a => a.Titulo)
                .Include(a => a.Resumo)
                .Include(a => a.Tipo)
                .Include(a => a.DataCriacao)
                .Slice(a => a.Midias, 0, 1);

            // *** CORREÇÃO ***
            var find = (session != null)
                ? _artigos.Find(session, filter)
                : _artigos.Find(filter);

            var models = await find
                .Project<ArtigoModel>(projection)
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

            // *** CORREÇÃO ***
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
            if (!ObjectId.TryParse(artigo.Id, out var objectId)) return false;
            var session = GetSession(sessionHandle);
            var model = _mapper.Map<ArtigoModel>(artigo);

            var result = (session != null)
                ? await _artigos.ReplaceOneAsync(session, a => a.Id == objectId.ToString(), model)
                : await _artigos.ReplaceOneAsync(a => a.Id == objectId.ToString(), model);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> UpdateMetricsAsync(string id, int totalComentarios, int totalInteracoes, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;
            var session = GetSession(sessionHandle);
            var filter = Builders<ArtigoModel>.Filter.Eq(a => a.Id, objectId.ToString());

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
            if (!ObjectId.TryParse(id, out var objectId)) return false;
            var session = GetSession(sessionHandle);

            var result = (session != null)
                ? await _artigos.DeleteOneAsync(session, a => a.Id == objectId.ToString())
                : await _artigos.DeleteOneAsync(a => a.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }
    }
}