using System;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa a interacao de um usuario com um artigo, como comentarios ou avaliacoes.
    /// Esta colecao gerencia tanto comentarios publicos quanto comentarios editoriais internos.
    /// </sumario>
    public class Interaction
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // Referencia ao Artigo principal ao qual a interacao se aplica.
        public string ArtigoId { get; set; } = string.Empty;

        // Referencia ao ID do usuário que fez o comentário (ID do sistema externo UsuarioApi).
        public string UsuarioId { get; set; } = string.Empty;

        // NOVO: Nome de exibição do usuário (obtido do UsuarioAPI no momento da criação).
        public string UsuarioNome { get; set; } = string.Empty;

        // Conteudo do comentário.
        public string Content { get; set; } = string.Empty;

        // Tipo de interação (Comentario Publico, Comentario Editorial, Like, etc.).
        public TipoInteracao Type { get; set; } = TipoInteracao.ComentarioPublico; // CORRIGIDO: TipoInteracao.ComentarioPublico

        // Threading: Referencia ao ID do comentário pai (se for uma resposta). 
        // Se for um comentário raiz, o valor sera 'string.Empty' ou 'null'.
        public string? ParentCommentId { get; set; }

        // Metadados
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // Para comentarios que podem ser editados.
        public DateTime? DataUltimaEdicao { get; set; }
    }
}