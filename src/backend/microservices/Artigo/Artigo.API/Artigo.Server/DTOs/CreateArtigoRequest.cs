using Artigo.Intf.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// Data Transfer Object (DTO) de entrada para a Mutação de criação de Artigo.
    /// Define os campos mínimos que o cliente deve fornecer para submeter um novo artigo.
    /// </sumario>
    public class CreateArtigoRequest
    {
        // Metadados principais
        [Required(ErrorMessage = "O título do artigo é obrigatório.")]
        [MaxLength(250, ErrorMessage = "O título não pode exceder 250 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O resumo do artigo é obrigatório.")]
        public string Resumo { get; set; } = string.Empty;

        // Conteúdo Principal do Artigo
        [Required(ErrorMessage = "O conteúdo do artigo é obrigatório.")]
        public string Conteudo { get; set; } = string.Empty;

        // Tipo de artigo (Enum)
        public TipoArtigo Tipo { get; set; } = TipoArtigo.Artigo;

        // Autores: O cliente envia a lista de DTOs de Autor, contendo ID, Nome e URL.
        public List<AutorInputDTO> Autores { get; set; } = [];

        // Campo 'AutorReference' mantido para autores não cadastrados.
        public List<string> ReferenciasAutor { get; set; } = [];

        // Mídias associadas ao artigo (imagem de destaque, etc.)
        public List<MidiaEntryInputDTO> Midias { get; set; } = [];
    }
}