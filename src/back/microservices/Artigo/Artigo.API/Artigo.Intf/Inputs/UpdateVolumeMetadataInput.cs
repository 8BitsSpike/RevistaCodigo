using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Artigo.Intf.Inputs
{
    /// <sumario>
    /// Define os campos que podem ser modificados em um Volume.
    /// Esta classe vive no 'Intf' (Domain) para ser usada pela IArtigoService.
    /// </sumario>
    public class UpdateVolumeMetadataInput
    {
        // Metadados da Publicação
        [Range(1, int.MaxValue, ErrorMessage = "A edição deve ser maior que zero.")]
        public int? Edicao { get; set; }

        [MaxLength(250, ErrorMessage = "O título não pode exceder 250 caracteres.")]
        public string? VolumeTitulo { get; set; }

        public string? VolumeResumo { get; set; }

        public MesVolume? M { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O número do volume deve ser maior que zero.")]
        public int? N { get; set; }

        [Range(2000, 3000, ErrorMessage = "O ano deve ser válido.")]
        public int? Year { get; set; }

        public StatusVolume? Status { get; set; }

        public MidiaEntry? ImagemCapa { get; set; }

        public List<string>? ArtigoIds { get; set; }
    }
}