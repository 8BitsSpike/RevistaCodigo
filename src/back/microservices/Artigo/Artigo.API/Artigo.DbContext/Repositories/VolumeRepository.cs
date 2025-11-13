using Artigo.DbContext.Data;
using Artigo.DbContext.Interfaces;
using Artigo.DbContext.PersistenceModels;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Artigo.DbContext.Repositories
{
    public class VolumeRepository : IVolumeRepository
    {
        private readonly IMongoCollection<VolumeModel> _volumes;
        private readonly IMapper _mapper;

        public VolumeRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _volumes = dbContext.Volumes;
            _mapper = mapper;
        }

        private IClientSessionHandle? GetSession(object? sessionHandle)
        {
            return (IClientSessionHandle?)sessionHandle;
        }

        // Define a projeção para os campos do 'Volume Card'
        private readonly ProjectionDefinition<VolumeModel> _volumeCardProjection =
            Builders<VolumeModel>.Projection
                .Include(v => v.Id)
                .Include(v => v.VolumeTitulo)
                .Include(v => v.VolumeResumo)
                .Include(v => v.ImagemCapa);

        // --- Implementação dos Métodos da Interface ---

        public async Task<Artigo.Intf.Entities.Volume?> GetByIdAsync(string id, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _volumes.Find(session, v => v.Id == objectId.ToString())
                : _volumes.Find(v => v.Id == objectId.ToString());

            var model = await find.FirstOrDefaultAsync();
            return _mapper.Map<Artigo.Intf.Entities.Volume>(model);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Volume>> GetByYearAsync(int year, int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _volumes.Find(session, v => v.Year == year)
                : _volumes.Find(v => v.Year == year);

            var models = await find
                .SortByDescending(v => v.DataCriacao)
                .ThenByDescending(v => v.M)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Volume>>(models);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Volume>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var filter = MongoDB.Driver.Builders<Artigo.DbContext.PersistenceModels.VolumeModel>.Filter.In(v => v.Id, ids);

            var find = (session != null)
                ? _volumes.Find(session, filter)
                : _volumes.Find(filter);

            var models = await find
                .SortByDescending(v => v.DataCriacao)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Volume>>(models);
        }

        /// <sumario>
        /// Retorna a lista de volumes (formato card) para o STAFF (sem filtro de status).
        /// </sumario>
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Volume>> GetAllAsync(int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _volumes.Find(session, _ => true)
                : _volumes.Find(_ => true);

            var models = await find
                .Project<VolumeModel>(_volumeCardProjection) // PROJEÇÃO ATUALIZADA!
                .SortByDescending(v => v.DataCriacao)
                .ThenByDescending(v => v.Year)
                .ThenByDescending(v => v.M)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Volume>>(models);
        }

        /// <sumario>
        /// Retorna a lista de volumes (formato card) para o PÚBLICO (apenas 'Publicado').
        /// </sumario>
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Volume>> ObterVolumesListAsync(int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            // FILTRO ADICIONADO
            var filter = Builders<VolumeModel>.Filter.Eq(v => v.Status, StatusVolume.Publicado);

            var find = (session != null)
                ? _volumes.Find(session, filter)
                : _volumes.Find(filter);

            var models = await find
                .Project<VolumeModel>(_volumeCardProjection) 
                .SortByDescending(v => v.DataCriacao)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Volume>>(models);
        }

        /// <sumario>
        /// Retorna volumes para o 'Card List Format' filtrados por StatusVolume.
        /// </sumario>
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Volume>> ObterVolumesPorStatusAsync(StatusVolume status, int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            var filter = Builders<VolumeModel>.Filter.Eq(v => v.Status, status);

            var find = (session != null)
                ? _volumes.Find(session, filter)
                : _volumes.Find(filter);

            var models = await find
                .Project<VolumeModel>(_volumeCardProjection)
                .SortByDescending(v => v.DataCriacao)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Volume>>(models);
        }


        public async Task AddAsync(Artigo.Intf.Entities.Volume volume, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var model = _mapper.Map<VolumeModel>(volume);

            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = ObjectId.GenerateNewId().ToString();
            }
            if (model.DataCriacao == DateTime.MinValue)
            {
                model.DataCriacao = DateTime.UtcNow;
            }

            if (session != null)
                await _volumes.InsertOneAsync(session, model);
            else
                await _volumes.InsertOneAsync(model);

            _mapper.Map(model, volume);
        }

        public async Task<bool> UpdateAsync(Artigo.Intf.Entities.Volume volume, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(volume.Id, out var objectId))
            {
                return false;
            }
            var session = GetSession(sessionHandle);

            var model = _mapper.Map<VolumeModel>(volume);

            var result = (session != null)
                ? await _volumes.ReplaceOneAsync(session, v => v.Id == objectId.ToString(), model)
                : await _volumes.ReplaceOneAsync(v => v.Id == objectId.ToString(), model);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteAsync(string id, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;
            var session = GetSession(sessionHandle);

            var result = (session != null)
                ? await _volumes.DeleteOneAsync(session, v => v.Id == objectId.ToString())
                : await _volumes.DeleteOneAsync(v => v.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }
    }
}