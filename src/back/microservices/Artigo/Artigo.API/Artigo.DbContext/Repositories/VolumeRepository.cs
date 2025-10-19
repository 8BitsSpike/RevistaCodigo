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
    /// <sumario>
    /// Implementacao do contrato de persistencia IVolumeRepository.
    /// Responsavel por gerenciar os metadados de publicacao das edicoes da revista.
    /// </sumario>
    public class VolumeRepository : IVolumeRepository
    {
        private readonly IMongoCollection<VolumeModel> _volumes;
        private readonly IMapper _mapper;

        public VolumeRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _volumes = dbContext.Volumes;
            _mapper = mapper;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<Artigo.Intf.Entities.Volume?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;

            var model = await _volumes
                .Find(v => v.Id == objectId.ToString())
                .FirstOrDefaultAsync();

            return _mapper.Map<Artigo.Intf.Entities.Volume>(model);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Volume>> GetByYearAsync(int year)
        {
            var models = await _volumes
                .Find(v => v.Year == year)
                // CORRIGIDO: VolumeMes.M -> MesVolume.M
                .SortByDescending(v => v.M)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Volume>>(models);
        }

        /// <sumario>
        /// Retorna multiplos Volumes com base em uma lista de IDs.
        /// Essencial para DataLoaders.
        /// </sumario>
        /// <param name="ids">Lista de IDs de volumes.</param>
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Volume>> GetByIdsAsync(IReadOnlyList<string> ids)
        {
            // Implementação usando a cláusula $in do MongoDB para buscar em lote.
            var filter = MongoDB.Driver.Builders<Artigo.DbContext.PersistenceModels.VolumeModel>.Filter.In(v => v.Id, ids);
            var models = await _volumes.Find(filter).ToListAsync();

            // Mapeia de volta para a Entidade de Domínio.
            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Volume>>(models);
        }

        // Implementa o contrato IVolumeRepository
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Volume>> GetAllAsync()
        {
            var models = await _volumes
                .Find(_ => true) // Busca todos
                .SortByDescending(v => v.Year)
                // CORRIGIDO: VolumeMes.M -> MesVolume.M
                .ThenByDescending(v => v.M)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Volume>>(models);
        }

        public async Task AddAsync(Artigo.Intf.Entities.Volume volume)
        {
            var model = _mapper.Map<VolumeModel>(volume);

            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = ObjectId.GenerateNewId().ToString();
            }
            if (model.DataCriacao == DateTime.MinValue)
            {
                model.DataCriacao = DateTime.UtcNow;
            }

            await _volumes.InsertOneAsync(model);

            // Atualiza a entidade de domínio com a ID final
            _mapper.Map(model, volume);
        }

        public async Task<bool> UpdateAsync(Artigo.Intf.Entities.Volume volume)
        {
            if (!ObjectId.TryParse(volume.Id, out var objectId))
            {
                return false;
            }

            var model = _mapper.Map<VolumeModel>(volume);

            // Usa ReplaceOneAsync para atualizar o documento inteiro (metadados e ArtigoIds)
            var result = await _volumes.ReplaceOneAsync(
                v => v.Id == objectId.ToString(),
                model
            );

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;

            var result = await _volumes.DeleteOneAsync(v => v.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }
    }
}