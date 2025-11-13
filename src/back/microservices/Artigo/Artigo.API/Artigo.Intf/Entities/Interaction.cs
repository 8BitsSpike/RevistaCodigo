using System;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa a interação de um usuário com um artigo, como comentários ou avaliações(quando implementadas).
    /// Esta coleção gerencia tanto comentários públicos quanto comentarios editoriais internos.
    /// </sumario>
    public class Interaction
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // Referência ao Artigo principal ao qual a interação se aplica.
        public string ArtigoId { get; set; } = string.Empty;

        // Referência ao ID do usuário que fez o comentário (ID do sistema externo UsuarioApi).
        public string UsuarioId { get; set; } = string.Empty;

        // Nome de exibição do usuário.
        public string UsuarioNome { get; set; } = string.Empty;

        // Conteúdo do comentário.
        public string Content { get; set; } = string.Empty;

        // Tipo de interação (Comentário Público, Comentário Editorial, Like, etc.).
        public TipoInteracao Type { get; set; } = TipoInteracao.ComentarioPublico;

        // Threading: Referência ao ID do comentário "pai" (se for uma resposta). 
        // Se for um comentário raiz, o valor sera 'string.Empty' ou 'null'.
        public string? ParentCommentId { get; set; }

        // Metadados
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // Para comentários que podem ser editados.
        public DateTime? DataUltimaEdicao { get; set; }
    }
}