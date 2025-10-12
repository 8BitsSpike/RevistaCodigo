using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para operacoes de persistencia de dados na colecao Volume.
    /// Responsavel por gerenciar as edicoes publicadas da revista e seu indice de artigos.
    /// </sumario>
    public interface IVolumeRepository
    {
        /// <sumario>
        /// Retorna um Volume pelo seu ID.
        /// Usado para carregar os metadados da edicao e a lista de artigos.
        /// </sumario>
        /// <param name="id">O identificador hexadecimal do Volume.</param>
        Task<Volume?> GetByIdAsync(string id);

        /// <sumario>
        /// Retorna uma lista de Volumes com base no ano de publicacao.
        /// Usado para listar o arquivo historico da revista.
        /// </sumario>
        /// <param name="year">O ano de publicacao.</param>
        Task<IReadOnlyList<Volume>> GetByYearAsync(int year);

        /// <sumario>
        /// Retorna uma lista de Volumes, util para paginacao na listagem geral.
        /// </summary>
        Task<IReadOnlyList<Volume>> GetAllAsync();

        /// <sumario>
        /// Adiciona uma nova edicao (Volume).
        /// Requer permissao de Administrador ou EditorChefe.
        /// </summary>
        /// <param name="newVolume">O objeto Volume a ser criado.</param>
        Task AddAsync(Volume newVolume);

        /// <sumario>
        /// Atualiza os dados de um Volume (e.g., adicao/remocao de ArtigoId ou metadados).
        /// </summary>
        /// <param name="updatedVolume">O objeto Volume com os dados atualizados.</param>
        Task<bool> UpdateAsync(Volume updatedVolume);

        /// <sumario>
        /// Remove um Volume.
        /// </summary>
        /// <param name="id">O ID do Volume a ser removido.</param>
        Task<bool> DeleteAsync(string id);
    }
}
