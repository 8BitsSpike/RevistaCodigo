using Artigo.Intf.Enums; // Adicionado para acessar os nomes de enum corretos
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
        public StatusArtigo Status { get; set; } // FIX: ArtigoStatus -> StatusArtigo
        public TipoArtigo Tipo { get; set; } // FIX: ArtigoTipo -> TipoArtigo

        // Referencias de relacionamento
        // RENOMEADO: AutorIds -> IdsAutor
        public List<string> IdsAutor { get; set; } = []; // Autores cadastrados (UsuarioApi)
        // RENOMEADO: AutorReference -> ReferenciasAutor
        public List<string> ReferenciasAutor { get; set; } = []; // Autores nao cadastrados (Nome)

        // Referencias de ligacao 1:1 ou 1:N
        // RENOMEADO: EditorialId -> IdEditorial
        public string IdEditorial { get; set; } = string.Empty;
        // RENOMEADO: VolumeId -> IdVolume
        public string? IdVolume { get; set; } // Opcional, so existe se publicado

        // MUDANÇA: Lista de objetos Midia (já alterado)
        public List<MidiaEntryDTO> Midias { get; set; } = []; // ADICIONADO

        // Metricas Denormalizadas (Subset Pattern)
        public int TotalInteracoes { get; set; } = 0;
        public int TotalComentarios { get; set; } = 0;

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
        // RENOMEADO: MidiaID -> IdMidia
        public string IdMidia { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        // RENOMEADO: Alt -> TextoAlternativo
        public string TextoAlternativo { get; set; } = string.Empty;
    }
}