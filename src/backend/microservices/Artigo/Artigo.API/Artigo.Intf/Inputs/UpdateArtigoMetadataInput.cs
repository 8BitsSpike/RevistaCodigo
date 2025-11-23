using Artigo.Intf.Enums;
using System.Collections.Generic;

namespace Artigo.Intf.Inputs
{
    /// <sumario>
    /// Classe de entrada para a atualização de metadados de um Artigo.
    /// Define os campos que podem ser modificados após a criação.
    /// Esta classe vive no 'Intf' (Domain) para ser usada pela IArtigoService.
    /// </sumario>
    public class UpdateArtigoMetadataInput
    {
        // Metadados principais
        public string? Titulo { get; set; }
        public string? Resumo { get; set; }

        // Tipo de artigo (Enum)
        public TipoArtigo? Tipo { get; set; }

        // Autores: Permite adicionar ou remover autores/co-autores, mas como uma lista completa.
        public List<string>? IdsAutor { get; set; }
        public List<string>? ReferenciasAutor { get; set; }

        // Novos campos para gerenciamento de status e comentários
        public StatusArtigo? Status { get; set; }
        public bool? PermitirComentario { get; set; }

        /// <sumario>
        /// (NOVO) Opcional: Define a posição do artigo no fluxo de trabalho editorial.
        /// </sumario>
        public PosicaoEditorial? Posicao { get; set; }
    }
}