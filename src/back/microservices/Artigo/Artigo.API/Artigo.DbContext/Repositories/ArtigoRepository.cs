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
    /// Implementacao do contrato de persistencia IArtigoRepository.
    /// Responsavel por mapear Entidades de Dominio para Modelos de Persistencia (MongoDB)
    /// e executar comandos no banco de dados.
    /// </sumario>
    public class ArtigoRepository : IArtigoRepository
    {
        private readonly IMongoCollection<ArtigoModel> _artigos;
        private readonly IMapper _mapper;

        /// <sumario>
        /// O construtor recebe a interface do contexto de dados (IMongoDbContext)
        /// e o IMapper, injetados pelo ASP.NET Core.
        /// </sumario>
        public ArtigoRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _artigos = dbContext.Artigos;
            _mapper = mapper;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<Artigo.Intf.Entities.Artigo?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;

            var model = await _artigos.Find(a => a.Id == objectId.ToString()).FirstOrDefaultAsync();

            return _mapper.Map<Artigo.Intf.Entities.Artigo>(model);
        }

        // NOVO MÉTODO (Implementa o contrato IArtigoRepository)
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> GetByStatusAsync(ArtigoStatus status)
        {
            var filter = Builders<ArtigoModel>.Filter.Eq(a => a.Status, status);
            var models = await _artigos.Find(filter).ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }

        // NOVO MÉTODO (Implementa o contrato IArtigoRepository)
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> GetByIdsAsync(IReadOnlyList<string> ids)
        {
            // O MongoDB Driver lida com a lista de strings (IDs) na clausula $in
            var filter = Builders<ArtigoModel>.Filter.In(a => a.Id, ids);
            var models = await _artigos.Find(filter).ToListAsync();

            return _mapper.Map<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(models);
        }

        public async Task AddAsync(Artigo.Intf.Entities.Artigo artigo)
        {
            var model = _mapper.Map<ArtigoModel>(artigo);

            await _artigos.InsertOneAsync(model);

            // Atualiza a entidade de domínio com o ID gerado (que o MongoDB setou no model)
            _mapper.Map(model, artigo);
        }

        public async Task<bool> UpdateAsync(Artigo.Intf.Entities.Artigo artigo)
        {
            if (!ObjectId.TryParse(artigo.Id, out var objectId)) return false;

            var model = _mapper.Map<ArtigoModel>(artigo);

            // O ReplaceOne substitui o documento inteiro
            var result = await _artigos.ReplaceOneAsync(
                a => a.Id == objectId.ToString(),
                model
            );

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        // NOVO MÉTODO (Implementa o contrato IArtigoRepository)
        public async Task<bool> UpdateMetricsAsync(string id, int totalComentarios, int totalInteracoes)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;

            var filter = Builders<ArtigoModel>.Filter.Eq(a => a.Id, objectId.ToString());

            // Define os campos a serem atualizados atomicamente
            var update = Builders<ArtigoModel>.Update
                .Set(a => a.TotalComentarios, totalComentarios)
                .Set(a => a.TotalInteracoes, totalInteracoes);

            var result = await _artigos.UpdateOneAsync(filter, update);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;

            var result = await _artigos.DeleteOneAsync(a => a.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }
    }
}