using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usuario.Intf.Models
{
    public class Atuacao
    {

        // Campos de Informações Atuação

        public string? Instituicao { get; set; } = string.Empty;

        public string? AreaAtuacao { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public string? DataInicio { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public string? DataFim { get; set; } = string.Empty;

        public string? Contribuicao { get; set; } = string.Empty;

        public string? InformacoesAdd { get; set; } = string.Empty;
    }
}
