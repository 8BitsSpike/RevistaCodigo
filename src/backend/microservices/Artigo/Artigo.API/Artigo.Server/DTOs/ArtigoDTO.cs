using Artigo.Intf.Enums; // Adicionado para acessar os nomes de enum corretos
using System.Collections.Generic;
using System;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// Data Transfer Object (DTO) para o Artigo.
    /// Usado para representar o Artigo na camada de apresentação (GraphQL).
    /// Contem apenas campos necessários para a visualização pública.
    /// </sumario>
    public class ArtigoDTO
    {
        // O ID é sempre string na camada DTO/API (Hexadecimal)
        public string Id { get; set; } = string.Empty;

        // Conteudo principal e metadata
        public string Titulo { get; set; } = string.Empty;
        public string Resumo { get; set; } = string.Empty;

        // Status e tipo
        public StatusArtigo Status { get; set; }
        public TipoArtigo Tipo { get; set; }

        // Referencias de relacionamento
        public List<string> IdsAutor { get; set; } = []; // Autores cadastrados (UsuarioApi)
        public List<string> ReferenciasAutor { get; set; } = []; // Autores nao cadastrados (Nome)

        // Referencias de ligacao 1:1 ou 1:N
        public string EditorialId { get; set; } = string.Empty;
        public string? IdVolume { get; set; } // Opcional, so existe se publicado

        // Lista de objetos Midia (já alterado)
        public List<MidiaEntryDTO> Midias { get; set; } = [];

        // Metricas Denormalizadas (Subset Pattern)
        public int TotalInteracoes { get; set; } = 0;
        public int TotalComentarios { get; set; } = 0;
        public bool PermitirComentario { get; set; }

        // Datas
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataPublicacao { get; set; }
        public DateTime? DataEdicao { get; set; }
        public DateTime? DataAcademica { get; set; }
    }

    /// <sumario>
    /// Data Transfer Object (DTO) para uma entrada de Midia (URL, Alt Text, ID).
    /// </sumario>
    public class MidiaEntryDTO
    {
        public string IdMidia { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string TextoAlternativo { get; set; } = string.Empty;
    }
}