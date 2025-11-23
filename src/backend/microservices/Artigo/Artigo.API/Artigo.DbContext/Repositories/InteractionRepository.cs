using Artigo.DbContext.Data;
using Artigo.DbContext.Interfaces;
using Artigo.DbContext.PersistenceModels;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using AutoMapper;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Artigo.DbContext.Repositories
{
    public class InteractionRepository : IInteractionRepository
    {
        private readonly IMongoCollection<InteractionModel> _interactions;
        private readonly IMapper _mapper;

        public InteractionRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _interactions = dbContext.Interactions;
            _mapper = mapper;
        }

        private IClientSessionHandle? GetSession(object? sessionHandle)
        {
            return (IClientSessionHandle?)sessionHandle;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<Artigo.Intf.Entities.Interaction?> GetByIdAsync(string id, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _interactions.Find(session, i => i.Id == objectId.ToString())
                : _interactions.Find(i => i.Id == objectId.ToString());

            var model = await find.FirstOrDefaultAsync();
            return _mapper.Map<Artigo.Intf.Entities.Interaction>(model);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Interaction>> GetByArtigoIdAsync(string artigoId, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _interactions.Find(session, i => i.ArtigoId == artigoId)
                : _interactions.Find(i => i.ArtigoId == artigoId);

            var models = await find
                .SortByDescending(i => i.DataCriacao)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Interaction>>(models);
        }

        public async Task<IReadOnlyList<Intf.Entities.Interaction>> GetByParentIdsAsync(IReadOnlyList<string> parentIds, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var filter = Builders<InteractionModel>.Filter.In(i => i.ParentCommentId, parentIds);

            var find = (session != null)
                ? _interactions.Find(session, filter)
                : _interactions.Find(filter);

            var models = await find
                .SortByDescending(i => i.DataCriacao)
                .ToListAsync();
            return _mapper.Map<IReadOnlyList<Intf.Entities.Interaction>>(models);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Interaction>> GetByArtigoIdsAsync(IReadOnlyList<string> artigoIds, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var filter = Builders<InteractionModel>.Filter.In(i => i.ArtigoId, artigoIds);

            var find = (session != null)
                ? _interactions.Find(session, filter)
                : _interactions.Find(filter);

            var models = await find
                .SortByDescending(i => i.DataCriacao)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Interaction>>(models);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Interaction>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var filter = Builders<InteractionModel>.Filter.In(i => i.Id, ids);

            var find = (session != null)
                ? _interactions.Find(session, filter)
                : _interactions.Find(filter);

            var models = await find
                .SortByDescending(i => i.DataCriacao)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Interaction>>(models);
        }

        public async Task AddAsync(Artigo.Intf.Entities.Interaction interaction, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var model = _mapper.Map<InteractionModel>(interaction);

            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = ObjectId.GenerateNewId().ToString();
            }
            if (model.DataCriacao == DateTime.MinValue)
            {
                model.DataCriacao = DateTime.UtcNow;
            }

            if (session != null)
                await _interactions.InsertOneAsync(session, model);
            else
                await _interactions.InsertOneAsync(model);

            _mapper.Map(model, interaction);
        }

        public async Task<bool> UpdateAsync(Artigo.Intf.Entities.Interaction interaction, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(interaction.Id, out var objectId)) return false;
            var session = GetSession(sessionHandle);

            var model = _mapper.Map<InteractionModel>(interaction);

            var result = (session != null)
                ? await _interactions.ReplaceOneAsync(session, i => i.Id == objectId.ToString(), model)
                : await _interactions.ReplaceOneAsync(i => i.Id == objectId.ToString(), model);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteAsync(string id, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;
            var session = GetSession(sessionHandle);

            var result = (session != null)
                ? await _interactions.DeleteOneAsync(session, i => i.Id == objectId.ToString())
                : await _interactions.DeleteOneAsync(i => i.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }

        public async Task<bool> DeleteByArtigoIdAsync(string artigoId, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            var result = (session != null)
                ? await _interactions.DeleteManyAsync(session, i => i.ArtigoId == artigoId)
                : await _interactions.DeleteManyAsync(i => i.ArtigoId == artigoId);

            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        /// <sumario>
        /// Retorna uma lista paginada de comentários públicos para um artigo específico.
        /// </sumario>
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Interaction>> GetPublicCommentsAsync(string artigoId, int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            // Filtro combinado para ArtigoId E TipoInteracao
            var filter = Builders<InteractionModel>.Filter.Eq(i => i.ArtigoId, artigoId) &
                         Builders<InteractionModel>.Filter.Eq(i => i.Type, TipoInteracao.ComentarioPublico);

            var find = (session != null)
                ? _interactions.Find(session, filter)
                : _interactions.Find(filter);

            var models = await find
                .SortByDescending(i => i.DataCriacao) // Ordena pelos mais recentes
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Interaction>>(models);
        }
    }
}