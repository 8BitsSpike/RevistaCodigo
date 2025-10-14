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
    /// Implementacao do contrato de persistencia IAutorRepository.
    /// Gerencia a ligacao entre o UsuarioId externo e o historico de contribuicoes.
    /// </sumario>
    public class AutorRepository : IAutorRepository
    {
        private readonly IMongoCollection<AutorModel> _autores;
        private readonly IMapper _mapper;

        public AutorRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _autores = dbContext.Autores;
            _mapper = mapper;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<Autor?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;

            var model = await _autores
                .Find(a => a.Id == objectId.ToString())
                .FirstOrDefaultAsync();

            return _mapper.Map<Autor>(model);
        }

        public async Task<IReadOnlyList<Autor>> GetByIdsAsync(IReadOnlyList<string> ids)
        {
            var filter = Builders<AutorModel>.Filter.In(a => a.Id, ids);
            var models = await _autores.Find(filter).ToListAsync();

            return _mapper.Map<IReadOnlyList<Autor>>(models);
        }

        public async Task<Autor?> GetByUsuarioIdAsync(string usuarioId)
        {
            var filter = Builders<AutorModel>.Filter.Eq(a => a.UsuarioId, usuarioId);
            var model = await _autores.Find(filter).FirstOrDefaultAsync();

            return _mapper.Map<Autor>(model);
        }

        // NOVO MÉTODO (Substitui AddAsync e UpdateAsync)
        public async Task<Autor> UpsertAsync(Autor autor)
        {
            var model = _mapper.Map<AutorModel>(autor);

            // Filtra pelo UsuarioId (chave externa) para encontrar o registro
            var filter = Builders<AutorModel>.Filter.Eq(a => a.UsuarioId, autor.UsuarioId);

            // Opções para o Upsert: 
            // - IsUpsert = true: Insere se não encontrar, atualiza se encontrar.
            // - ReturnDocument.After: Retorna o documento após a operação.
            var options = new FindOneAndReplaceOptions<AutorModel>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            // Se o registro for novo, model.Id será string.Empty, e o MongoDB irá gerar um ObjectId.
            // Se for uma atualização, model.Id manterá o ObjectId existente.
            var upsertedModel = await _autores.FindOneAndReplaceAsync(filter, model, options);

            // Mapeia o modelo final (com a ID e o estado corretos) de volta para a Entidade de Domínio.
            return _mapper.Map<Autor>(upsertedModel);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;

            var result = await _autores.DeleteOneAsync(a => a.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }
    }
}