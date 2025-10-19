using Artigo.Intf.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// Data Transfer Object (DTO) de entrada para a Mutacao de criacao de Artigo.
    /// Define os campos minimos que o cliente deve fornecer para submeter um novo artigo.
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
        public string Conteudo { get; set; } = string.Empty; // CAMPO ADICIONADO

        // Tipo de artigo (Enum)
        public TipoArtigo Tipo { get; set; } = TipoArtigo.Artigo; // FIX: ArtigoTipo -> TipoArtigo

        // Autores: O cliente envia a lista de IDs de usuários cadastrados e os nomes dos não-cadastrados.
        // RENOMEADO: AutorIds -> IdsAutor
        public List<string> IdsAutor { get; set; } = [];
        // RENOMEADO: AutorReference -> ReferenciasAutor
        public List<string> ReferenciasAutor { get; set; } = [];
    }
}