using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// DTO principal para o "Artigo Editorial View".
    /// Agrega todas as informações necessárias para a página de edição de um artigo
    /// por um membro da equipe editorial ou autor.
    /// </sumario>
   
    public class ArtigoEditorialViewDTO
    {
        // Campos do Artigo
        public string Id { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Resumo { get; set; } = string.Empty;
        public TipoArtigo Tipo { get; set; }
        public StatusArtigo Status { get; set; }
        public List<string> AutorIds { get; set; } = []; // IDs para o resolver de Autores
        public List<string> AutorReferencias { get; set; } = []; // Nomes
        public string EditorialId { get; set; } = string.Empty; // ID para resolvers
        public string? VolumeId { get; set; } // ID para o resolver de Volume
        public bool PermitirComentario { get; set; }

        // --- Campos Resolvidos (preenchidos por resolvers no GraphQL) ---

        // Dados do Editorial (de EditorialId)
        public EditorialViewDTO? Editorial { get; set; }

        // Dados do History (de Editorial.CurrentHistoryId)
        public ArtigoHistoryEditorialViewDTO? ConteudoAtual { get; set; }

        // Dados do Volume (de VolumeId)
        public VolumeCardDTO? Volume { get; set; }

        // Dados de Interação (de ArtigoId e EditorialId)
        public InteractionConnectionDTO? Interacoes { get; set; }
    }

    /// <sumario>
    /// DTO aninhado para os dados do Editorial.
    /// </sumario>
    public class EditorialViewDTO
    {
        public PosicaoEditorial Position { get; set; }
        public string CurrentHistoryId { get; set; } = string.Empty;
        public EditorialTeam Team { get; set; } = new EditorialTeam();
    }

    /// <sumario>
    /// DTO aninhado para o conteúdo do ArtigoHistory na visão editorial.
    /// </sumario>
    public class ArtigoHistoryEditorialViewDTO
    {
        public VersaoArtigo Version { get; set; }
        public string Content { get; set; } = string.Empty;
        public List<MidiaEntryDTO> Midias { get; set; } = [];
        public List<StaffComentario> StaffComentarios { get; set; } = [];
    }
}