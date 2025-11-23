using System.Collections.Generic;
using Artigo.Intf.Enums;
using System;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa o ciclo de vida editorial e o estado de revisão de um artigo.
    /// Contem todas as referências à revisores, corretores, e o histórico de versão.
    /// </sumario>
    public class Editorial
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // Referência ao Artigo principal (ligacao 1:1)
        public string ArtigoId { get; set; } = string.Empty;

        // Posicao atual do artigo no ciclo de revisão.
        public PosicaoEditorial Position { get; set; } = PosicaoEditorial.AguardandoRevisao;

        // Versão atual do corpo do artigo sendo trabalhada (refêrencia a ArtigoHistory).
        public string CurrentHistoryId { get; set; } = string.Empty;

        // Coleção de IDs para rastrear todas as versões do corpo do artigo.
        // Referência a coleção ArtigoHistory.
        public List<string> HistoryIds { get; set; } = [];

        // Coleção de comentários feitos pelos editores/revisores em cada iteracao.
        // Referência a coleção Interaction/Comments.
        public List<string> CommentIds { get; set; } = [];

        // Equipe editorial responsável pelo artigo neste ciclo.
        public EditorialTeam Team { get; set; } = new EditorialTeam();

        // Data da ultima vez que a posição editorial foi atualizada.
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <sumario>
    /// Objeto embutido para gerenciar a equipe de revisão e edição.
    /// Os IDs referenciam a coleção Autor (que contem o UsuarioId externo).
    /// </sumario>
    public class EditorialTeam
    {
        // Lista de IDs dos autores do artigo (Referencia a UsuarioId)
        public List<string> InitialAuthorId { get; set; } = [];

        // ID do editor chefe responsavel (Referencia a Staff.Id)
        public List<string> EditorIds { get; set; } = [];

        // IDs dos revisores designados (Referencia a UsuarioId)
        public List<string> ReviewerIds { get; set; } = [];

        // IDs dos corretores designados (Referencia a UsuarioId)
        public List<string> CorrectorIds { get; set; } = [];
    }
}