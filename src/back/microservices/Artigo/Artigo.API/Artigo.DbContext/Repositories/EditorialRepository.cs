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
    public class EditorialRepository : IEditorialRepository
    {
        private readonly IMongoCollection<EditorialModel> _editoriais;
        private readonly IMapper _mapper;

        public EditorialRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _editoriais = dbContext.Editoriais;
            _mapper = mapper;
        }

        private IClientSessionHandle? GetSession(object? sessionHandle)
        {
            return (IClientSessionHandle?)sessionHandle;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<Editorial?> GetByIdAsync(string id, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _editoriais.Find(session, e => e.Id == objectId.ToString())
                : _editoriais.Find(e => e.Id == objectId.ToString());

            var model = await find.FirstOrDefaultAsync();
            return _mapper.Map<Editorial>(model);
        }

        public async Task<Editorial?> GetByArtigoIdAsync(string artigoId, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _editoriais.Find(session, e => e.ArtigoId == artigoId)
                : _editoriais.Find(e => e.ArtigoId == artigoId);

            var model = await find.FirstOrDefaultAsync();
            return _mapper.Map<Editorial>(model);
        }

        public async Task<IReadOnlyList<Editorial>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var filter = Builders<EditorialModel>.Filter.In(e => e.Id, ids);

            var find = (session != null)
                ? _editoriais.Find(session, filter)
                : _editoriais.Find(filter);

            var models = await find
                .SortByDescending(e => e.LastUpdated)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Editorial>>(models);
        }

        public async Task AddAsync(Editorial editorial, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var model = _mapper.Map<EditorialModel>(editorial);

            if (session != null)
                await _editoriais.InsertOneAsync(session, model);
            else
                await _editoriais.InsertOneAsync(model);

            _mapper.Map(model, editorial);
        }

        // --- Metodos de Atualizacao Granular ---

        public async Task<bool> UpdatePositionAsync(string editorialId, PosicaoEditorial newPosition, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(editorialId, out var objectId)) return false;
            var session = GetSession(sessionHandle);

            var update = Builders<EditorialModel>.Update
                .Set(e => e.Position, newPosition)
                .Set(e => e.LastUpdated, DateTime.UtcNow);

            var result = (session != null)
                ? await _editoriais.UpdateOneAsync(session, e => e.Id == objectId.ToString(), update)
                : await _editoriais.UpdateOneAsync(e => e.Id == objectId.ToString(), update);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> UpdateHistoryAsync(string editorialId, string newHistoryId, List<string> allHistoryIds, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(editorialId, out var objectId)) return false;
            var session = GetSession(sessionHandle);

            var update = Builders<EditorialModel>.Update
                .Set(e => e.CurrentHistoryId, newHistoryId)
                .Set(e => e.HistoryIds, allHistoryIds)
                .Set(e => e.LastUpdated, DateTime.UtcNow);

            var result = (session != null)
                ? await _editoriais.UpdateOneAsync(session, e => e.Id == objectId.ToString(), update)
                : await _editoriais.UpdateOneAsync(e => e.Id == objectId.ToString(), update);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> AddCommentIdAsync(string editorialId, string commentId, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(editorialId, out var objectId)) return false;
            var session = GetSession(sessionHandle);

            var update = Builders<EditorialModel>.Update.Push(e => e.CommentIds, commentId);

            var result = (session != null)
                ? await _editoriais.UpdateOneAsync(session, e => e.Id == objectId.ToString(), update)
                : await _editoriais.UpdateOneAsync(e => e.Id == objectId.ToString(), update);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> UpdateTeamAsync(string editorialId, EditorialTeam team, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(editorialId, out var objectId)) return false;
            var session = GetSession(sessionHandle);

            var teamModel = _mapper.Map<EditorialTeamModel>(team);

            var update = Builders<EditorialModel>.Update
                .Set(e => e.Team, teamModel)
                .Set(e => e.LastUpdated, DateTime.UtcNow);

            var result = (session != null)
                ? await _editoriais.UpdateOneAsync(session, e => e.Id == objectId.ToString(), update)
                : await _editoriais.UpdateOneAsync(e => e.Id == objectId.ToString(), update);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        // --- Metodos de Remoção ---

        public async Task<bool> DeleteAsync(string id, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;
            var session = GetSession(sessionHandle);

            var result = (session != null)
                ? await _editoriais.DeleteOneAsync(session, e => e.Id == objectId.ToString())
                : await _editoriais.DeleteOneAsync(e => e.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }

        public async Task<bool> DeleteByArtigoIdAsync(string artigoId, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            var result = (session != null)
                ? await _editoriais.DeleteOneAsync(session, e => e.ArtigoId == artigoId)
                : await _editoriais.DeleteOneAsync(e => e.ArtigoId == artigoId);

            return result.IsAcknowledged && result.DeletedCount == 1;
        }
    }
}