using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para operacoes de persistencia de dados na colecao Autor.
    /// Esta colecao atua como uma tabela de ligacao para o serviço de Usuario externo.
    /// </sumario>
    public interface IAutorRepository
    {
        /// <sumario>
        /// Retorna um registro de Autor pelo seu ID local.
        /// </sumario>
        /// <param name="id">O identificador hexadecimal local do autor.</param>
        Task<Autor?> GetByIdAsync(string id);

        /// <sumario>
        /// Retorna um registro de Autor usando o ID do sistema externo (UsuarioId).
        /// </sumario>
        /// <param name="usuarioId">O identificador do autor no sistema externo.</param>
        Task<Autor?> GetByUsuarioIdAsync(string usuarioId);

        /// <sumario>
        /// Retorna multiplos registros de Autor com base em uma lista de IDs locais.
        /// Essencial para DataLoaders.
        /// </sumario>
        /// <param name="ids">Lista de IDs locais de autores.</param>
        Task<IReadOnlyList<Autor>> GetByIdsAsync(IReadOnlyList<string> ids);

        /// <sumario>
        /// Adiciona um novo registro de Autor, ou atualiza um registro existente.
        /// Usado para garantir que o autor existe antes de adicionar novas contribuicoes.
        /// </sumario>
        /// <param name="autor">O objeto Autor a ser criado ou atualizado.</param>
        Task<Autor> UpsertAsync(Autor autor);

        /// <sumario>
        /// Remove um registro de Autor.
        /// </sumario>
        /// <param name="id">O ID do registro de Autor a ser removido.</param>
        Task<bool> DeleteAsync(string id);
    }
}