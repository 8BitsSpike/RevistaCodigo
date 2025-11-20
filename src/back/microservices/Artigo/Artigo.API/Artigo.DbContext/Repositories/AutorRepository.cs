using Artigo.DbContext.Data;
using Artigo.DbContext.Interfaces;
using Artigo.DbContext.PersistenceModels;
using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Artigo.DbContext.Repositories
{
    public class AutorRepository : IAutorRepository
    {
        private readonly IMongoCollection<AutorModel> _autores;
        private readonly IMapper _mapper;

        public AutorRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _autores = dbContext.Autores;
            _mapper = mapper;
        }

        private IClientSessionHandle? GetSession(object? sessionHandle)
        {
            return (IClientSessionHandle?)sessionHandle;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<Autor?> GetByIdAsync(string id, object? sessionHandle = null)
        {
            if (string.IsNullOrEmpty(id)) return null;

            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _autores.Find(session, a => a.Id == id)
                : _autores.Find(a => a.Id == id);

            var model = await find.FirstOrDefaultAsync();
            return _mapper.Map<Autor>(model);
        }

        public async Task<IReadOnlyList<Autor>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var filter = Builders<AutorModel>.Filter.In(a => a.Id, ids);

            var find = (session != null)
                ? _autores.Find(session, filter)
                : _autores.Find(filter);

            var models = await find
                .SortByDescending(a => a.Id)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Autor>>(models);
        }

        public async Task<Autor?> GetByUsuarioIdAsync(string usuarioId, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var filter = Builders<AutorModel>.Filter.Eq(a => a.UsuarioId, usuarioId);

            var find = (session != null)
                ? _autores.Find(session, filter)
                : _autores.Find(filter);

            var model = await find.FirstOrDefaultAsync();
            return _mapper.Map<Autor>(model);
        }

        public async Task<IReadOnlyList<Autor>> GetAllAsync(int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _autores.Find(session, _ => true)
                : _autores.Find(_ => true);

            var models = await find
                .SortByDescending(a => a.Id)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Autor>>(models);
        }

        /// <sumario>
        /// Usa UpdateOneAsync com IsUpsert=true para uma operação de upsert atômica e segura,
        /// evitando o erro "immutable _id field".
        /// </sumario>
        public async Task<Autor> UpsertAsync(Autor autor, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var model = _mapper.Map<AutorModel>(autor);

            var filter = Builders<AutorModel>.Filter.Eq(a => a.UsuarioId, autor.UsuarioId);

            var update = Builders<AutorModel>.Update
                .Set(a => a.Nome, model.Nome)
                .Set(a => a.Url, model.Url)
                .Set(a => a.ArtigoWorkIds, model.ArtigoWorkIds)
                .Set(a => a.Contribuicoes, model.Contribuicoes)
                .SetOnInsert(a => a.Id, ObjectId.GenerateNewId().ToString())
                .SetOnInsert(a => a.UsuarioId, model.UsuarioId);

            var options = new UpdateOptions { IsUpsert = true };

            if (session != null)
                await _autores.UpdateOneAsync(session, filter, update, options);
            else
                await _autores.UpdateOneAsync(filter, update, options);
            var upsertedModel = await GetByUsuarioIdAsync(autor.UsuarioId, sessionHandle);

            return upsertedModel!;
        }

        public async Task<bool> DeleteAsync(string id, object? sessionHandle = null)
        {
            if (string.IsNullOrEmpty(id)) return false;
            var session = GetSession(sessionHandle);

            var result = (session != null)
                ? await _autores.DeleteOneAsync(session, a => a.Id == id)
                : await _autores.DeleteOneAsync(a => a.Id == id);

            return result.IsAcknowledged && result.DeletedCount == 1;
        }

        /// <sumario>
        /// Busca autores (registrados) pelo nome, usando regex.
        /// </sumario>
        public async Task<IReadOnlyList<Autor>> SearchAutoresByNameAsync(string searchTerm, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            string escapedSearchTerm = System.Text.RegularExpressions.Regex.Escape(searchTerm);
            var filter = Builders<AutorModel>.Filter.Regex(a => a.Nome, new BsonRegularExpression($".*{escapedSearchTerm}.*", "i"));

            var find = (session != null)
                ? _autores.Find(session, filter)
                : _autores.Find(filter);

            var models = await find
                .SortByDescending(a => a.Id)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Autor>>(models);
        }
    }
}