using System;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa uma versao historica do corpo do artigo para fins de rastreamento editorial.
    /// Esta entidade guarda o conteudo completo de uma versao especifica.
    /// </sumario>
    public class ArtigoHistory
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // Referencia ao Artigo principal ao qual esta versao pertence.
        public string ArtigoId { get; set; } = string.Empty;

        // Versao do Artigo (0 - Original, 1- 1a Edicao, 2 - 2a Edicao, 3 - 3a Edicao e 4 - Edicao Final)
        public ArtigoVersion Version { get; set; }

        // O conteudo completo do artigo nesta versao.
        public string Content { get; set; } = string.Empty;

        // Data e hora em que esta versao foi registrada.
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    }
}
