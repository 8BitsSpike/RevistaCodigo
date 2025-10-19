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
    /// <sumario>
    /// Implementacao do contrato de persistencia IEditorialRepository.
    /// Gerencia o ciclo de vida editorial, posição e referencias ao historico do artigo.
    /// </sumario>
    public class EditorialRepository : IEditorialRepository
    {
        private readonly IMongoCollection<EditorialModel> _editoriais;
        private readonly IMapper _mapper;

        public EditorialRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _editoriais = dbContext.Editoriais;
            _mapper = mapper;
        }

        // --- Implementação dos Métodos da Interface ---

        // NOVO MÉTODO (Implementa o contrato IEditorialRepository)
        public async Task<Editorial?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;

            var model = await _editoriais
                .Find(e => e.Id == objectId.ToString())
                .FirstOrDefaultAsync();

            return _mapper.Map<Editorial>(model);
        }

        public async Task<Editorial?> GetByArtigoIdAsync(string artigoId)
        {
            var model = await _editoriais
                .Find(e => e.ArtigoId == artigoId)
                .FirstOrDefaultAsync();

            return _mapper.Map<Editorial>(model);
        }

        public Task<IReadOnlyList<Editorial>> GetByIdsAsync(IReadOnlyList<string> ids)
        {
            var filter = Builders<EditorialModel>.Filter.In(e => e.Id, ids);
            var models = _editoriais.Find(filter).ToListAsync();

            return Task.FromResult(_mapper.Map<IReadOnlyList<Editorial>>(models.Result));
        }

        public async Task AddAsync(Editorial editorial)
        {
            var model = _mapper.Map<EditorialModel>(editorial);
            await _editoriais.InsertOneAsync(model);

            _mapper.Map(model, editorial);
        }

        // --- Metodos de Atualizacao Granular ---

        // CORRIGIDO: EditorialPosition -> PosicaoEditorial
        public async Task<bool> UpdatePositionAsync(string editorialId, PosicaoEditorial newPosition)
        {
            if (!ObjectId.TryParse(editorialId, out var objectId)) return false;

            var update = Builders<EditorialModel>.Update
                .Set(e => e.Position, newPosition)
                .Set(e => e.LastUpdated, DateTime.UtcNow);

            var result = await _editoriais.UpdateOneAsync(e => e.Id == objectId.ToString(), update);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> UpdateHistoryAsync(string editorialId, string newHistoryId, List<string> allHistoryIds)
        {
            if (!ObjectId.TryParse(editorialId, out var objectId)) return false;

            var update = Builders<EditorialModel>.Update
                .Set(e => e.CurrentHistoryId, newHistoryId)
                .Set(e => e.HistoryIds, allHistoryIds)
                .Set(e => e.LastUpdated, DateTime.UtcNow);

            var result = await _editoriais.UpdateOneAsync(e => e.Id == objectId.ToString(), update);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> AddCommentIdAsync(string editorialId, string commentId)
        {
            if (!ObjectId.TryParse(editorialId, out var objectId)) return false;

            // Adiciona o novo ID de comentário à lista CommentIds atomicamente
            var update = Builders<EditorialModel>.Update.Push(e => e.CommentIds, commentId);

            var result = await _editoriais.UpdateOneAsync(e => e.Id == objectId.ToString(), update);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> UpdateTeamAsync(string editorialId, EditorialTeam team)
        {
            if (!ObjectId.TryParse(editorialId, out var objectId)) return false;

            // Mapeia a entidade embutida para o modelo embutido
            var teamModel = _mapper.Map<EditorialTeamModel>(team);

            // Define o objeto embutido 'Team'
            var update = Builders<EditorialModel>.Update
                .Set(e => e.Team, teamModel)
                .Set(e => e.LastUpdated, DateTime.UtcNow);

            var result = await _editoriais.UpdateOneAsync(e => e.Id == objectId.ToString(), update);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        // --- Metodos de Remocao ---

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;

            var result = await _editoriais.DeleteOneAsync(e => e.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }

        // NOVO MÉTODO (Implementa o contrato IEditorialRepository)
        public async Task<bool> DeleteByArtigoIdAsync(string artigoId)
        {
            var result = await _editoriais.DeleteOneAsync(e => e.ArtigoId == artigoId);

            return result.IsAcknowledged && result.DeletedCount == 1;
        }
    }
}