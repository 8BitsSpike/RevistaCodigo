using Artigo.DbContext.Data;
using Artigo.DbContext.Interfaces;
using Artigo.DbContext.PersistenceModels;
using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;
using Artigo.Intf.Enums;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Artigo.DbContext.Repositories
{
    public class ArtigoHistoryRepository : IArtigoHistoryRepository
    {
        private readonly IMongoCollection<ArtigoHistoryModel> _history;
        private readonly IMapper _mapper;

        public ArtigoHistoryRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _history = dbContext.ArtigoHistories;
            _mapper = mapper;
        }

        private IClientSessionHandle? GetSession(object? sessionHandle)
        {
            return (IClientSessionHandle?)sessionHandle;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<ArtigoHistory?> GetByIdAsync(string id, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _history.Find(session, h => h.Id == objectId.ToString())
                : _history.Find(h => h.Id == objectId.ToString());

            var model = await find.FirstOrDefaultAsync();
            return _mapper.Map<ArtigoHistory>(model);
        }

        public async Task<IReadOnlyDictionary<string, string>> GetContentsByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var filter = Builders<ArtigoHistoryModel>.Filter.In(h => h.Id, ids);

            var projection = Builders<ArtigoHistoryModel>.Projection
                .Include(h => h.Id)
                .Include(h => h.Content);

            var find = (session != null)
                ? _history.Find(session, filter)
                : _history.Find(filter);

            var models = await find
                .Project(projection)
                .ToListAsync();

            return models
                .Where(doc => doc.Contains("Content") && !doc["Content"].IsBsonNull)
                .ToDictionary(
                    doc => doc["_id"].AsString,
                    doc => doc["Content"].AsString
                );
        }

        public async Task<ArtigoHistory?> GetByArtigoAndVersionAsync(string artigoId, VersaoArtigo version, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var filter = Builders<ArtigoHistoryModel>.Filter.Eq(h => h.ArtigoId, artigoId) &
                         Builders<ArtigoHistoryModel>.Filter.Eq(h => h.Version, version);

            var find = (session != null)
                ? _history.Find(session, filter)
                : _history.Find(filter);

            var model = await find
                .SortByDescending(h => h.DataRegistro)
                .FirstOrDefaultAsync();

            return _mapper.Map<ArtigoHistory>(model);
        }

        public async Task<IReadOnlyList<ArtigoHistory>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var filter = Builders<ArtigoHistoryModel>.Filter.In(h => h.Id, ids);

            var find = (session != null)
                ? _history.Find(session, filter)
                : _history.Find(filter);

            var models = await find
                .SortByDescending(h => h.DataRegistro)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<ArtigoHistory>>(models);
        }

        public async Task AddAsync(ArtigoHistory history, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var model = _mapper.Map<ArtigoHistoryModel>(history);

            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = ObjectId.GenerateNewId().ToString();
            }

            foreach (var comentario in model.StaffComentarios)
            {
                if (string.IsNullOrEmpty(comentario.Id))
                {
                    comentario.Id = ObjectId.GenerateNewId().ToString();
                }
            }

            if (session != null)
                await _history.InsertOneAsync(session, model);
            else
                await _history.InsertOneAsync(model);

            _mapper.Map(model, history);
        }

        public async Task<bool> UpdateAsync(ArtigoHistory historyEntry, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(historyEntry.Id, out var objectId)) return false;
            var session = GetSession(sessionHandle);

            var model = _mapper.Map<ArtigoHistoryModel>(historyEntry);

            foreach (var comentario in model.StaffComentarios)
            {
                if (string.IsNullOrEmpty(comentario.Id))
                {
                    comentario.Id = ObjectId.GenerateNewId().ToString();
                }
            }

            var result = (session != null)
                ? await _history.ReplaceOneAsync(session, h => h.Id == objectId.ToString(), model)
                : await _history.ReplaceOneAsync(h => h.Id == objectId.ToString(), model);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteByArtigoIdAsync(string artigoId, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            var result = (session != null)
                ? await _history.DeleteManyAsync(session, h => h.ArtigoId == artigoId)
                : await _history.DeleteManyAsync(h => h.ArtigoId == artigoId);

            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}