using System;
using System.Collections.Generic;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa uma edicao publicada da revista (Volume).
    /// Contem o indice de todos os artigos publicados naquela edicao.
    /// </sumario>
    public class Volume
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // Metadados da Publicacao
        public int Edicao { get; set; } // O numero sequencial desta edicao da revista.
        public string VolumeTitulo { get; set; } = string.Empty; // Titulo desta edicao
        public string VolumeResumo { get; set; } = string.Empty; // Resumo do contepudo desta edicao
        public VolumeMes M { get; set; } // O mes de publicacao (Enum).
        public int N { get; set; } // O numero do volume (mantido por compatibilidade historica).
        public int Year { get; set; } // O ano da publicacao.

        // Referencia a colecao Artigo.IDs de todos os artigos publicados neste volume.
        // Usado para listar o indice da revista e para DataLoaders.
        public List<string> ArtigoIds { get; set; } = [];

        // Metadados
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
