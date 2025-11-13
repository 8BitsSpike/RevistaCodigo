using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using System;
using System.Collections.Generic;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// DTO para a "Volume View" (visualização completa de um volume).
    /// Agrega todas as informações públicas de um volume, incluindo a lista de ArtigoIds
    /// para ser resolvida pelo GraphQL.
    /// </sumario>
    public class VolumeViewDTO
    {
        // Identificador
        public string Id { get; set; } = string.Empty;

        // Metadados da Publicação
        public int Edicao { get; set; }
        public string VolumeTitulo { get; set; } = string.Empty;
        public string VolumeResumo { get; set; } = string.Empty;
        public MesVolume M { get; set; }
        public int N { get; set; }
        public int Year { get; set; }

        // Status (para o frontend saber se é 'Publicado')
        public StatusVolume Status { get; set; }

        // Mídia de capa
        public MidiaEntry? ImagemCapa { get; set; }

        // Lista de Artigos (para o resolver)
        public List<string> ArtigoIds { get; set; } = [];

        // Metadados
        public DateTime DataCriacao { get; set; }
    }
}