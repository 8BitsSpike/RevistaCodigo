using MongoDB.Driver;
using Artigo.Intf.Entities;
using System.Threading.Tasks;
using Artigo.DbContext.PersistenceModels;

namespace Artigo.DbContext.Interfaces
{
    /// <sumario>
    /// Contrato para o contexto de dados do MongoDB.
    /// Expoem as coleções (que usam os MODELOS DE PERSISTENCIA como tipo) para injeção nos repositórios.
    /// NOTA CRÍTICA: O uso de PersistenceModels aqui é uma exceção à regra de Clean Architecture, 
    /// imposta pela necessidade de tipagem do driver do MongoDB.
    /// </sumario>
    public interface IMongoDbContext
    {
        // Tipos alterados para os Persistence Models (*Model)
        IMongoCollection<ArtigoModel> Artigos { get; }
        IMongoCollection<AutorModel> Autores { get; }
        IMongoCollection<EditorialModel> Editoriais { get; }
        IMongoCollection<InteractionModel> Interactions { get; }
        IMongoCollection<ArtigoHistoryModel> ArtigoHistories { get; }
        IMongoCollection<PendingModel> Pendings { get; }
        IMongoCollection<StaffModel> Staffs { get; }
        IMongoCollection<VolumeModel> Volumes { get; }
    }
}