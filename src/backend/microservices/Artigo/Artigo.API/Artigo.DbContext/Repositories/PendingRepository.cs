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
    public class PendingRepository : IPendingRepository
    {
        private readonly IMongoCollection<PendingModel> _pendings;
        private readonly IMapper _mapper;

        public PendingRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _pendings = dbContext.Pendings;
            _mapper = mapper;
        }

        private IClientSessionHandle? GetSession(object? sessionHandle)
        {
            return (IClientSessionHandle?)sessionHandle;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<Pending?> GetByIdAsync(string id, object? sessionHandle = null)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _pendings.Find(session, p => p.Id == id)
                : _pendings.Find(p => p.Id == id);

            var model = await find.FirstOrDefaultAsync();
            return _mapper.Map<Pending>(model);
        }

        public async Task<IReadOnlyList<Pending>> BuscarPendenciasPorStatus(StatusPendente status, int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _pendings.Find(session, p => p.Status == status)
                : _pendings.Find(p => p.Status == status);

            var models = await find
                .SortByDescending(p => p.DateRequested)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Pending>>(models);
        }

        public async Task<IReadOnlyList<Pending>> GetAllAsync(int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _pendings.Find(session, _ => true)
                : _pendings.Find(_ => true);

            var models = await find
                .SortByDescending(p => p.DateRequested)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Pending>>(models);
        }

        public async Task<IReadOnlyList<Pending>> BuscarPendenciaPorEntidadeId(string targetEntityId, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _pendings.Find(session, p => p.TargetEntityId == targetEntityId)
                : _pendings.Find(p => p.TargetEntityId == targetEntityId);

            var models = await find
                .SortByDescending(p => p.DateRequested)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Pending>>(models);
        }

        public async Task<IReadOnlyList<Pending>> BuscarPendenciaPorTipoDeEntidade(TipoEntidadeAlvo targetType, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _pendings.Find(session, p => p.TargetType == targetType)
                : _pendings.Find(p => p.TargetType == targetType);

            var models = await find
                .SortByDescending(p => p.DateRequested)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Pending>>(models);
        }

        public async Task<IReadOnlyList<Pending>> BuscarPendenciaPorRequisitanteId(string requesterUsuarioId, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _pendings.Find(session, p => p.RequesterUsuarioId == requesterUsuarioId)
                : _pendings.Find(p => p.RequesterUsuarioId == requesterUsuarioId);

            var models = await find
                .SortByDescending(p => p.DateRequested)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Pending>>(models);
        }

        public async Task AddAsync(Pending pending, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var model = _mapper.Map<PendingModel>(pending);

            if (model.DateRequested == DateTime.MinValue)
            {
                model.DateRequested = DateTime.UtcNow;
            }

            model.Status = StatusPendente.AguardandoRevisao;

            if (session != null)
                await _pendings.InsertOneAsync(session, model);
            else
                await _pendings.InsertOneAsync(model);

            _mapper.Map(model, pending);
        }

        public async Task<bool> UpdateAsync(Pending pending, object? sessionHandle = null)
        {
            if (string.IsNullOrEmpty(pending.Id)) return false;
            var session = GetSession(sessionHandle);

            var model = _mapper.Map<PendingModel>(pending);

            var result = (session != null)
                ? await _pendings.ReplaceOneAsync(session, p => p.Id == pending.Id, model)
                : await _pendings.ReplaceOneAsync(p => p.Id == pending.Id, model);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteAsync(string id, object? sessionHandle = null)
        {
            if (string.IsNullOrEmpty(id)) return false;
            var session = GetSession(sessionHandle);

            var result = (session != null)
                ? await _pendings.DeleteOneAsync(session, p => p.Id == id)
                : await _pendings.DeleteOneAsync(p => p.Id == id);

            return result.IsAcknowledged && result.DeletedCount == 1;
        }
    }
}