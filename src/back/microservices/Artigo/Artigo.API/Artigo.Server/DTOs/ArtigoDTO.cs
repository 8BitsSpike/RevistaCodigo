using Artigo.Intf.Enums;
using System.Collections.Generic;
using System;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// Data Transfer Object (DTO) para o Artigo.
    /// Usado para representar o Artigo na camada de apresentacao (GraphQL).
    /// Contem apenas campos necessarios para a visualizacao publica.
    /// </sumario>
    public class ArtigoDTO
    {
        // O ID é sempre string na camada DTO/API (Hexadecimal)
        public string Id { get; set; } = string.Empty;

        // Conteudo principal e metadata
        public string Titulo { get; set; } = string.Empty;
        public string Resumo { get; set; } = string.Empty;

        // Status e tipo
        public ArtigoStatus Status { get; set; }
        public ArtigoTipo Tipo { get; set; }

        // Referencias de relacionamento
        public List<string> AutorIds { get; set; } = []; // Autores cadastrados (UsuarioApi)
        public List<string> AutorReference { get; set; } = []; // Autores nao cadastrados (Nome)

        // Referencias de ligacao 1:1 ou 1:N
        public string EditorialId { get; set; } = string.Empty;
        public string? VolumeId { get; set; } // Opcional, so existe se publicado
        public List<string> MidiaIds { get; set; } = []; // Referencias a colecao Midia

        // Metricas Denormalizadas (Subset Pattern)
        public int TotalInteracoes { get; set; } = 0;
        public int TotalComentarios { get; set; } = 0;

        // Datas
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataPublicacao { get; set; }
        public DateTime? DataEdicao { get; set; }
        public DateTime? DataAcademica { get; set; }
    }
}
