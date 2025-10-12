using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para operacoes de persistencia de dados na colecao Interaction.
    /// É responsavel por gerenciar comentarios publicos e comentarios internos (editoriais).
    /// </sumario>
    public interface IInteractionRepository
    {
        /// <sumario>
        /// Retorna um comentario pelo seu ID.
        /// Usado para verificar a existencia e o tipo (e.g., se é ComentarioEditorial, que não permite respostas).
        /// </sumario>
        /// <param name="id">O identificador hexadecimal do comentario.</param>
        Task<Interaction?> GetByIdAsync(string id);

        /// <sumario>
        /// Retorna todos os comentarios (publicos e editoriais) associados a um Artigo.
        /// Essencial para resolvers que precisam carregar todos os comentarios de um artigo.
        /// </sumario>
        /// <param name="artigoId">O ID do artigo principal.</param>
        Task<IReadOnlyList<Interaction>> GetByArtigoIdAsync(string artigoId);

        /// <sumario>
        /// Retorna multiplos comentarios com base em uma lista de IDs.
        /// Crucial para DataLoaders que buscam respostas aninhadas ou coleções de comentários.
        /// </sumario>
        /// <param name="ids">Lista de IDs de interacoes.</param>
        Task<IReadOnlyList<Interaction>> GetByIdsAsync(IReadOnlyList<string> ids);

        /// <sumario>
        /// Adiciona um novo comentario (seja publico ou editorial).
        /// </sumario>
        /// <param name="interaction">O objeto Interaction a ser criado.</param>
        Task AddAsync(Interaction interaction);

        /// <sumario>
        /// Atualiza um comentario existente.
        /// Geralmente usado para moderacao/edicao de conteudo.
        /// </sumario>
        /// <param name="interaction">O objeto Interaction com os dados atualizados.</param>
        Task<bool> UpdateAsync(Interaction interaction);

        /// <sumario>
        /// Remove um comentario.
        /// Usado para remocao de spam ou comentarios rejeitados pelo editorial.
        /// </sumario>
        /// <param name="id">O ID do comentario a ser removido.</param>
        Task<bool> DeleteAsync(string id);
    }
}
