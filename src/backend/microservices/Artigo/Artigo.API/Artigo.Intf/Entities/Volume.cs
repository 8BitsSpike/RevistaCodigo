using System;
using System.Collections.Generic;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa uma edição publicada da revista (Volume).
    /// Contem o índice de todos os artigos publicados naquela edição.
    /// </sumario>
    public class Volume
    {
        // Identificador do Domínio.
        public string Id { get; set; } = string.Empty;

        // Metadados da Publicação
        public int Edicao { get; set; } // O número sequencial desta edição da revista.
        public string VolumeTitulo { get; set; } = string.Empty; // Titulo desta edicao
        public string VolumeResumo { get; set; } = string.Empty; // Resumo do conteúdo desta edicao
        public MesVolume M { get; set; } // MesVolume, mes de publicação (Enum).
        public int N { get; set; } // O número do volume (mantido por compatibilidade histórica).
        public int Year { get; set; } // O ano da publicação.

        // Status do ciclo de vida do volume
        public StatusVolume Status { get; set; } = StatusVolume.EmRevisao;

        // Mídia de capa para esta edição.
        public MidiaEntry? ImagemCapa { get; set; }

        // Referência a coleção Artigo.IDs de todos os artigos publicados neste volume.
        // Usado para listar o índice da revista e para DataLoaders.
        public List<string> ArtigoIds { get; set; } = [];

        // Metadados
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}