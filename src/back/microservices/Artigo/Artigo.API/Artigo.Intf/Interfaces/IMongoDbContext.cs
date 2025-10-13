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
        IMongoCollection<Artigo.Intf.Entities.Artigo> Artigos { get; }
        // Uso do endereço literal do Artigo type porque tanto ele quanto o namespace Artigo tem a mesma grafia
        // Poderiamos usar ArtigoType para o tipo Artigo, mas esse tipo de nomeclatura não é ideal
        IMongoCollection<Autor> Autores { get; }
        IMongoCollection<Editorial> Editoriais { get; }
        IMongoCollection<Interaction> Interactions { get; }
        IMongoCollection<ArtigoHistory> ArtigoHistories { get; }
        IMongoCollection<Pending> Pendings { get; }
        IMongoCollection<Staff> Staffs { get; }
        IMongoCollection<Volume> Volumes { get; }
    }
}