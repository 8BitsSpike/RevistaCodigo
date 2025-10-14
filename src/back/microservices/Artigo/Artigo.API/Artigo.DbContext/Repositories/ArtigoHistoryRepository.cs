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

namespace Artigo.DbContext.Repositories
{
    /// <sumario>
    /// Implementacao do contrato de persistencia IArtigoHistoryRepository.
    /// Gerencia o conteudo das versoes historicas do artigo.
    /// </sumario>
    public class ArtigoHistoryRepository : IArtigoHistoryRepository
    {
        private readonly IMongoCollection<ArtigoHistoryModel> _history;
        private readonly IMapper _mapper;

        public ArtigoHistoryRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _history = dbContext.ArtigoHistories;
            _mapper = mapper;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<ArtigoHistory?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;

            var model = await _history
                .Find(h => h.Id == objectId.ToString())
                .FirstOrDefaultAsync();

            return _mapper.Map<ArtigoHistory>(model);
        }

        // NOVO MÉTODO (Implementa o contrato IArtigoHistoryRepository)
        public async Task<ArtigoHistory?> GetByArtigoAndVersionAsync(string artigoId, Artigo.Intf.Enums.ArtigoVersion version)
        {
            var filter = Builders<ArtigoHistoryModel>.Filter.Eq(h => h.ArtigoId, artigoId) &
                         Builders<ArtigoHistoryModel>.Filter.Eq(h => h.Version, version);

            var model = await _history.Find(filter).FirstOrDefaultAsync();

            return _mapper.Map<ArtigoHistory>(model);
        }

        // NOVO MÉTODO (Implementa o contrato IArtigoHistoryRepository)
        public async Task<IReadOnlyList<ArtigoHistory>> GetByIdsAsync(IReadOnlyList<string> ids)
        {
            var filter = Builders<ArtigoHistoryModel>.Filter.In(h => h.Id, ids);
            var models = await _history.Find(filter).ToListAsync();

            return _mapper.Map<IReadOnlyList<ArtigoHistory>>(models);
        }

        public async Task AddAsync(ArtigoHistory history)
        {
            var model = _mapper.Map<ArtigoHistoryModel>(history);

            // Garante que a ID seja gerada se for nova
            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = ObjectId.GenerateNewId().ToString();
            }

            await _history.InsertOneAsync(model);

            // Atualiza a entidade de domínio com a ID final
            _mapper.Map(model, history);
        }

        // NOVO MÉTODO (Implementa o contrato IArtigoHistoryRepository)
        public async Task<bool> UpdateAsync(ArtigoHistory historyEntry)
        {
            if (!ObjectId.TryParse(historyEntry.Id, out var objectId)) return false;

            var model = _mapper.Map<ArtigoHistoryModel>(historyEntry);

            var result = await _history.ReplaceOneAsync(
                h => h.Id == objectId.ToString(),
                model
            );

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;

            var result = await _history.DeleteOneAsync(h => h.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }

        // Método auxiliar existente no arquivo original, útil para serviços de limpeza.
        public async Task<bool> DeleteByArtigoIdAsync(string artigoId)
        {
            // Deleta todo o historico de versoes de um artigo
            var result = await _history.DeleteManyAsync(h => h.ArtigoId == artigoId);

            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}