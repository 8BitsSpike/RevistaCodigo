using MongoDB.Driver;
using Artigo.Intf.Entities;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Contrato para o contexto de dados do MongoDB.
    /// Expoem as colecoes (que usam as ENTIDADES DE DOMINIO como tipo) para injecao nos repositorios.
    /// A conversao para Modelos de Persistencia (ArtigoModel) é responsabilidade dos Repositorios.
    /// </sumario>
    public interface IMongoDbContext
    {
        IMongoCollection<Artigo> Artigos { get; }
        IMongoCollection<Autor> Autores { get; }
        IMongoCollection<Editorial> Editoriais { get; }
        IMongoCollection<Interaction> Interactions { get; }
        IMongoCollection<ArtigoHistory> ArtigoHistories { get; }
        IMongoCollection<Pending> Pendings { get; }
        IMongoCollection<Staff> Staffs { get; }
        IMongoCollection<Volume> Volumes { get; }
    }
}