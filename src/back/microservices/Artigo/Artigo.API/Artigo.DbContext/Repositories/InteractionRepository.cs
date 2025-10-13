using Artigo.DbContext.Data;
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
    /// <sumario>
    /// Implementacao do contrato de persistencia IInteractionRepository.
    /// Gerencia comentarios publicos e editoriais e suas hierarquias.
    /// </sumario>
    public class InteractionRepository : IInteractionRepository
    {
        private readonly IMongoCollection<InteractionModel> _interactions;
        private readonly IMapper _mapper;

        public InteractionRepository(IMongoDbContext dbContext, IMapper mapper)
        {
            _interactions = dbContext.Interactions;
            _mapper = mapper;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<Artigo.Intf.Entities.Interaction?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;

            var model = await _interactions
                .Find(i => i.Id == objectId.ToString())
                .FirstOrDefaultAsync();

            return _mapper.Map<Artigo.Intf.Entities.Interaction>(model);
        }

        // Implementa o contrato IInteractionRepository (que espera IReadOnlyList<Interaction>)
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Interaction>> GetByArtigoIdAsync(string artigoId)
        {
            // O serviço de aplicação é responsável por filtrar o tipo (público/editorial).
            // Aqui, retornamos todos os comentários associados ao ArtigoId.
            var models = await _interactions
                .Find(i => i.ArtigoId == artigoId)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Interaction>>(models);
        }

        // NOVO MÉTODO (Implementa o contrato IInteractionRepository para DataLoaders)
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Interaction>> GetByIdsAsync(IReadOnlyList<string> ids)
        {
            var filter = Builders<InteractionModel>.Filter.In(i => i.Id, ids);
            var models = await _interactions.Find(filter).ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Interaction>>(models);
        }

        // Método de filtro auxiliar (útil internamente, mas não no contrato IInteractionRepository)
        public async Task<List<Artigo.Intf.Entities.Interaction>> GetByArtigoIdAndTypeAsync(string artigoId, InteractionType typeFilter)
        {
            var filter = Builders<InteractionModel>.Filter.Eq(i => i.ArtigoId, artigoId) &
                         Builders<InteractionModel>.Filter.Eq(i => i.Type, typeFilter);

            var models = await _interactions
                .Find(filter)
                .ToListAsync();

            return _mapper.Map<List<Artigo.Intf.Entities.Interaction>>(models);
        }

        public async Task AddAsync(Artigo.Intf.Entities.Interaction interaction)
        {
            var model = _mapper.Map<InteractionModel>(interaction);

            // Garante que a ID seja gerada e a data de criação seja definida
            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = ObjectId.GenerateNewId().ToString();
            }
            if (model.DataCriacao == DateTime.MinValue)
            {
                model.DataCriacao = DateTime.UtcNow;
            }

            await _interactions.InsertOneAsync(model);

            // Atualiza a entidade de domínio com a ID final e data de criação
            _mapper.Map(model, interaction);
        }

        // Implementa o contrato IInteractionRepository (UpdateAsync)
        public async Task<bool> UpdateAsync(Artigo.Intf.Entities.Interaction interaction)
        {
            if (!ObjectId.TryParse(interaction.Id, out var objectId)) return false;

            // Mapeia a entidade para o modelo (necessário para atualizar o Content ou Type)
            var model = _mapper.Map<InteractionModel>(interaction);

            var result = await _interactions.ReplaceOneAsync(
                i => i.Id == objectId.ToString(),
                model
            );

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;

            var result = await _interactions.DeleteOneAsync(i => i.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }

        // Método auxiliar existente no arquivo original, útil para serviços de limpeza.
        public async Task<bool> DeleteByArtigoIdAsync(string artigoId)
        {
            // Deleta todos os comentários relacionados a um artigo (útil ao arquivar/deletar artigo)
            var result = await _interactions.DeleteManyAsync(i => i.ArtigoId == artigoId);

            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<IReadOnlyList<Intf.Entities.Interaction>> GetByArtigoIdsAsync(IReadOnlyList<string> artigoIds)
        {
            var filter = Builders<InteractionModel>.Filter.In(i => i.ArtigoId, artigoIds);
            var models = await _interactions.Find(filter).ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Interaction>>(models);
        }
        public async Task<IReadOnlyList<Intf.Entities.Interaction>> GetByParentIdsAsync(IReadOnlyList<string> parentIds)
        {
            var filter = Builders<InteractionModel>.Filter.In(i => i.ParentCommentId, parentIds);
            var models = await _interactions.Find(filter).ToListAsync();
            return _mapper.Map<IReadOnlyList<Intf.Entities.Interaction>>(models);
        }
    }
}