using System;
using Artigo.Intf.Enums;
using System.Collections.Generic; // Necessário para List<T>

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa uma versão histórica do corpo do artigo para fins de rastreamento editorial.
    /// Esta entidade guarda o conteudo completo de uma versão específica, incluindo o estado da midia associada.
    /// </sumario>
    public class ArtigoHistory
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // Referência ao Artigo principal ao qual esta verão pertence.
        public string ArtigoId { get; set; } = string.Empty;

        // Versão do Artigo (0 - Original, 1- 1a Edição, 2 - 2a Edição, 3 - 3a Edição e 4 - Edição Final)
        public VersaoArtigo Version { get; set; }

        // O conteúdo completo do artigo nesta versão.
        public string Content { get; set; } = string.Empty;

        // Lista de Midias associadas nesta versão do histórico.
        public List<MidiaEntry> Midias { get; set; } = [];

        // Lista de comentários internos da equipe editorial sobre esta versão.
        public List<StaffComentario> StaffComentarios { get; set; } = [];

        // Data e hora em que esta versão foi registrada.
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    }

    /// <sumario>
    /// Objeto embutido para rastrear os comentários internos da equipe editorial
    /// sobre uma versão específica do ArtigoHistory.
    /// </sumario>
    public class StaffComentario
    {
        // ID único do comentário (pode ser um ObjectId string)
        public string Id { get; set; } = string.Empty;

        // ID do usuário (Staff) que fez o comentário.
        public string UsuarioId { get; set; } = string.Empty;

        // Data e hora do comentário.
        public DateTime Data { get; set; } = DateTime.UtcNow;

        // ID do comentário "pai" para threading (se for uma resposta).
        public string? Parent { get; set; }

        // O conteúdo do comentário.
        public string Comment { get; set; } = string.Empty;
    }
}