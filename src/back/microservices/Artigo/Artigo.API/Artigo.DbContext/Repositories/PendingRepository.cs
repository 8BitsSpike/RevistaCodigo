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
    /// Implementacao do contrato de persistencia IPendingRepository.
    /// Gerencia a fila de requisicoes pendentes de aprovacao editorial.
    /// </sumario>
    public class PendingRepository : IPendingRepository
    {
        private readonly IMongoCollection<PendingModel> _pendings;
        private readonly IMapper _mapper;

        public PendingRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _pendings = dbContext.Pendings;
            _mapper = mapper;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<Pending?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;

            var model = await _pendings
                .Find(p => p.Id == objectId.ToString())
                .FirstOrDefaultAsync();

            return _mapper.Map<Pending>(model);
        }

        // CORRIGIDO: PendingStatus -> StatusPendente
        public async Task<IReadOnlyList<Pending>> GetByStatusAsync(StatusPendente status)
        {
            var models = await _pendings
                .Find(p => p.Status == status)
                .SortByDescending(p => p.DateRequested)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Pending>>(models);
        }

        // O método GetByTargetAsync (presente no arquivo original) não é parte da interface
        // IPendingRepository, mas é útil internamente. Mantê-lo aqui.
        // CORRIGIDO: TargetEntityType -> TipoEntidadeAlvo
        public async Task<IReadOnlyList<Pending>> GetByTargetAsync(TipoEntidadeAlvo type, string targetId)
        {
            // CORRIGIDO: TargetEntityType -> TipoEntidadeAlvo
            var models = await _pendings
                .Find(p => p.TargetType == type && p.TargetEntityId == targetId)
                .SortByDescending(p => p.DateRequested)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Pending>>(models);
        }

        public async Task AddAsync(Pending pending)
        {
            var model = _mapper.Map<PendingModel>(pending);

            // Garante que a ID seja gerada e a data de criação seja definida
            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = ObjectId.GenerateNewId().ToString();
            }
            if (model.DateRequested == DateTime.MinValue)
            {
                model.DateRequested = DateTime.UtcNow;
            }

            // CORRIGIDO: PendingStatus.AwaitingReview -> StatusPendente.AguardandoRevisao
            model.Status = StatusPendente.AguardandoRevisao; // Nova requisição sempre começa aqui

            await _pendings.InsertOneAsync(model);

            // Atualiza a entidade de domínio com a ID final e data de criação
            _mapper.Map(model, pending);
        }

        // Implementa o contrato IPendingRepository.UpdateAsync(Pending pending)
        public async Task<bool> UpdateAsync(Pending pending)
        {
            if (!ObjectId.TryParse(pending.Id, out var objectId)) return false;

            var model = _mapper.Map<PendingModel>(pending);

            // O ReplaceOne substitui o documento inteiro, usado para garantir que
            // todas as mudanças (status, comentários, etc.) sejam persistidas.
            var result = await _pendings.ReplaceOneAsync(
                p => p.Id == objectId.ToString(),
                model
            );

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;

            var result = await _pendings.DeleteOneAsync(p => p.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }
    }
}