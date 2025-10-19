using System.Collections.Generic;
using Artigo.Intf.Enums;
using System;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa o ciclo de vida editorial e o estado de revisao de um artigo.
    /// Contem todas as referencias a revisores, corretores, e o historico de versoes.
    /// </sumario>
    public class Editorial
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // Referencia ao Artigo principal (ligacao 1:1)
        public string ArtigoId { get; set; } = string.Empty;

        // Posicao atual do artigo no ciclo de revisao.
        public PosicaoEditorial Position { get; set; } = PosicaoEditorial.AguardandoRevisao; // CORRIGIDO: PosicaoEditorial.AguardandoRevisao

        // Versao atual do corpo do artigo sendo trabalhada (referencia a ArtigoHistory).
        public string CurrentHistoryId { get; set; } = string.Empty;

        // Colecao de IDs para rastrear todas as versoes do corpo do artigo.
        // Referencia a colecao ArtigoHistory.
        public List<string> HistoryIds { get; set; } = [];

        // Colecao de comentários feitos pelos editores/revisores em cada iteracao.
        // Referencia a colecao Interaction/Comments.
        public List<string> CommentIds { get; set; } = [];

        // Equipe editorial responsavel pelo artigo neste ciclo.
        public EditorialTeam Team { get; set; } = new EditorialTeam();

        // Data da ultima vez que a posição editorial foi atualizada.
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <sumario>
    /// Objeto embutido para gerenciar a equipe de revisao e edicao.
    /// Os IDs referenciam a colecao Autor (que contem o UsuarioId externo).
    /// </sumario>
    public class EditorialTeam
    {
        // Lista de IDs dos autores do artigo (Referencia a Autor.Id)
        public List<string> InitialAuthorId { get; set; } = [];

        // ID do editor chefe responsavel (Referencia a Staff.Id)
        public string EditorId { get; set; } = string.Empty;

        // IDs dos revisores designados (Referencia a Autor.Id)
        public List<string> ReviewerIds { get; set; } = [];

        // IDs dos corretores designados (Referencia a Autor.Id)
        public List<string> CorrectorIds { get; set; } = [];
    }
}