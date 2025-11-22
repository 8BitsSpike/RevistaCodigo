using Artigo.Intf.Enums;
using System.ComponentModel.DataAnnotations;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// Data Transfer Object (DTO) de entrada para a Mutação de criação de uma nova edição (Volume) da revista.
    /// Usado por Staff para iniciar uma nova pauta de publicação.
    /// </sumario>
    public class CreateVolumeRequest
    {
        /// <sumario>
        /// O número sequencial desta edição.
        /// </sumario>
        [Required(ErrorMessage = "O número da edição é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "A edição deve ser maior que zero.")]
        public int Edicao { get; set; }

        /// <sumario>
        /// O título temático ou principal desta edição.
        /// </sumario>
        [Required(ErrorMessage = "O título do volume é obrigatório.")]
        [MaxLength(250, ErrorMessage = "O título não pode exceder 250 caracteres.")]
        public string VolumeTitulo { get; set; } = string.Empty;

        /// <sumario>
        /// Resumo ou sinopse do conteúdo desta edição.
        /// </sumario>
        public string VolumeResumo { get; set; } = string.Empty;

        /// <sumario>
        /// O mês de publicação pretendido para esta edição (Enum MesVolume).
        /// </sumario>
        [Required(ErrorMessage = "O mês de publicação é obrigatório.")]
        public MesVolume M { get; set; }

        /// <sumario>
        /// O número do volume (mantido por compatibilidade histórica, geralmente anual).
        /// </sumario>
        [Required(ErrorMessage = "O número do volume (N) é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "O número do volume deve ser maior que zero.")]
        public int N { get; set; }

        /// <sumario>
        /// O ano de publicação desta edição.
        /// </sumario>
        [Required(ErrorMessage = "O ano é obrigatório.")]
        [Range(2000, 3000, ErrorMessage = "O ano deve ser válido.")]
        public int Year { get; set; }

        public MidiaEntryInputDTO? ImagemCapa { get; set; }
        // A lista de ArtigoIds será atualizada em mutações subsequentes.
    }
}