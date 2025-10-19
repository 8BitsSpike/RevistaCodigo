using System;
using Artigo.Intf.Enums;
using System.Collections.Generic; // Necessário para List<T>

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa uma versao historica do corpo do artigo para fins de rastreamento editorial.
    /// Esta entidade guarda o conteudo completo de uma versao especifica, incluindo o estado da midia associada.
    /// </sumario>
    public class ArtigoHistory
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // Referencia ao Artigo principal ao qual esta versao pertence.
        public string ArtigoId { get; set; } = string.Empty;

        // Versao do Artigo (0 - Original, 1- 1a Edicao, 2 - 2a Edicao, 3 - 3a Edicao e 4 - Edicao Final)
        public VersaoArtigo Version { get; set; } // CORRIGIDO: VersaoArtigo

        // O conteudo completo do artigo nesta versao.
        public string Content { get; set; } = string.Empty;

        // ADICIONADO: Lista de Midias associadas nesta versão do histórico.
        public List<MidiaEntry> Midias { get; set; } = [];

        // Data e hora em que esta versao foi registrada.
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    }
}