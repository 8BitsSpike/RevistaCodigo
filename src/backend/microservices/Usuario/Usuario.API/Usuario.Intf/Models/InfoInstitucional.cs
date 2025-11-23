using System.ComponentModel.DataAnnotations;

namespace Usuario.Intf.Models
{
    /// <summary>
    /// Esta classe representa um único registo de histórico académico/institucional.
    /// É o objeto que será armazenado na coleção 'InfoInstitucionais' dentro do documento de Usuário.
    /// </summary>
    public class InfoInstitucional
    {
        // Campos de Informações Institucionais

        public string? Instituicao { get; set; } = string.Empty;

        public string? Curso { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public string? DataInicio { get; set; } = string.Empty; 

        [DataType(DataType.Date)]
        public string? DataFim { get; set; } = string.Empty; 

        public string? DescricaoCurso { get; set; } = string.Empty;

        public string? InformacoesAdd { get; set; } = string.Empty;

    }
}
