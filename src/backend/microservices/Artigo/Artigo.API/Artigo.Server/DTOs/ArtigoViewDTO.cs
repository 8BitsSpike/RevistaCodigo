using Artigo.Intf.Entities;
using System.Collections.Generic;
using Artigo.Intf.Enums;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// DTO principal para o "Artigo View" (leitura de artigo).
    /// Agrega todas as informações necessárias para a página de um artigo.
    /// </sumario>
    public class ArtigoViewDTO
    {
        // Campos do Artigo
        public string Id { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public List<string> AutorIds { get; set; } = []; // IDs para o resolver de Autores
        public List<string> AutorReferencias { get; set; } = []; // Nomes de autores não-cadastrados
        public string? VolumeId { get; set; } // ID para o resolver de Volume
        public bool PermitirComentario { get; set; }

        public TipoArtigo Tipo { get; set; }

        // Campo do Editorial (para o resolver de History)
        public string EditorialId { get; set; } = string.Empty;

        // --- Campos Resolvidos (preenchidos por resolvers no GraphQL) ---

        // Dados dos Autores (de AutorIds)
        public List<AutorViewDTO> Autores { get; set; } = [];

        // Dados do Volume (de VolumeId)
        public VolumeCardDTO? Volume { get; set; }

        // Dados do History (de EditorialId)
        public ArtigoHistoryViewDTO? ConteudoAtual { get; set; }

        // Dados de Interação (de EditorialId e ArtigoId)
        public InteractionConnectionDTO? Interacoes { get; set; }
    }

    /// <sumario>
    /// DTO aninhado para o conteúdo do ArtigoHistory no ArtigoView.
    /// </sumario>
    public class ArtigoHistoryViewDTO
    {
        public string Content { get; set; } = string.Empty;
        public List<MidiaEntryDTO> Midias { get; set; } = [];
    }
}