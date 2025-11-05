using Artigo.Intf.Enums;
using System;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// Data Transfer Object (DTO) para o formato 'Volume List'.
    /// Contém os campos necessários para exibir uma entrada de volume em uma lista.
    /// </sumario>
    public class VolumeListDTO
    {
        public string Id { get; set; } = string.Empty;
        public int Edicao { get; set; }
        public string VolumeTitulo { get; set; } = string.Empty;
        public string VolumeResumo { get; set; } = string.Empty;
        public MesVolume M { get; set; }
        public int N { get; set; }
        public int Year { get; set; }
    }
}