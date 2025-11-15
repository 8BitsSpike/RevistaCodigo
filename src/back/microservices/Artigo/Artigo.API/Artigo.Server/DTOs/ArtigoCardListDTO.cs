using Artigo.Intf.Enums;
using System;
using System.Collections.Generic;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// Data Transfer Object (DTO) para o formato 'Card List'.
    /// Contém apenas os campos mínimos necessários para exibir um "card" de artigo em uma lista.
    /// </sumario>
    public class ArtigoCardListDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Resumo { get; set; } = string.Empty;
        public StatusArtigo Status { get; set; }
        public TipoArtigo Tipo { get; set; }
        public bool PermitirComentario { get; set; }

        // Mapeia a primeira mídia da lista (imagem de destaque)
        public MidiaEntryDTO? MidiaDestaque { get; set; }
    }
}