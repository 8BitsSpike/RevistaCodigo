namespace Artigo.Intf.Enums
{
    /// <sumario>
    /// Status do ciclo de vida editorial de um artigo.
    /// </sumario>
    public enum ArtigoStatus
    {
        Draft,
        AwaitingApproval,
        InReview,
        ReadyToPublish,
        Published,
        Archived
    }

    /// <sumario>
    /// Classificacao do tipo de artigo (Artigo, Blog, Entrevista, Indicacao e Opiniao).
    /// </sumario>
    public enum ArtigoTipo
    {
        Artigo,
        Blog,
        Entrevista,
        Indicacao,
        Opniao
    }

    /// <sumario>
    /// Versoes do corpo do artigo guardadas no historico.
    /// Mapeia para o campo 'Version' na colecao ArtigoHistory.
    /// </sumario>
    public enum ArtigoVersion
    {
        Original = 0,
        PrimeiraEdicao = 1,
        SegundaEdicao = 2,
        TerceiraEdicao = 3,
        QuartaEdicao = 4,
        Final = 5 // Este indica a versao que sera publicada
    }

    /// <sumario>
    /// Papel desempenhado por um Autor durante uma contribuicao especifica.
    /// </sumario>
    public enum ContribuicaoRole
    {
        AutorPrincipal,
        CoAutor,
        Revisor,
        Corretor,
        Redator,
        EditorChefe
    }

    /// <sumario>
    /// Posicao atual do Artigo no fluxo editorial.
    /// </sumario>
    public enum EditorialPosition
    {
        Submitted,              // Enviado pelo autor, aguardando triagem.
        AwaitingReview,         // Aguardando revisao pelos Revisores.
        ReviewComplete,         // Revisao concluída, aguardando correcao.
        AwaitingCorrection,     // Aguardando correcao pelo Corretor.
        CorrectionComplete,     // Correcao concluída, aguardando redacao.
        AwaitingRedaction,      // Aguardando redacao final.
        RedactionComplete,      // Redacao concluída, pronto para a pauta.
        Rejected,               // Rejeitado em qualquer fase.
        OnHold                  // Em espera.
    }

    /// <sumario>
    /// Classifica o tipo de interacao (comentario, like, etc.).
    /// </sumario>
    public enum InteractionType
    {
        ComentarioPublico,      // Comentário padrão de um usuário leitor.
        ComentarioEditorial    // Comentário feito por um membro da equipe editorial sobre o conteúdo.
    }

    /// <sumario>
    /// Status de uma requisição pendente.
    /// </sumario>
    public enum PendingStatus
    {
        AwaitingReview,         // Aguardando decisao de um editor ou administrador.
        Approved,               // Requisição aprovada e executada.
        Rejected,               // Requisição rejeitada.
        Archived                // Requisição antiga ou concluída.
    }

    /// <sumario>
    /// Tipo de entidade referenciada na requisição pendente.
    /// </sumario>
    public enum TargetEntityType
    {
        Artigo,                 // Ação afeta o artigo.
        Autor,                  // Ação afeta o registro local do autor.
        Comment,                // Ação afeta o comentario
        Staff,                  // Ação afeta o registro de staff (e.g., mudança de função).
        Volume                  // Ação afeta a publicação (e.g., reordenar artigos em uma edição).
    }

    /// <sumario>
    /// Funcao de um membro da equipe (Staff) para fins de autorizacao.
    /// Define o nivel de permissao para aprovar Pendings ou executar acoes criticas.
    /// </sumario>
    public enum JobRole
    {
        Administrador,         // Permissao total no sistema.
        EditorBolsista,        // Permissao para criar Pendings.
        EditorChefe            // Permissao para gerenciar revisores e aprovar Pendings.
    }

    /// <sumario>
    /// Mês de publicacao para a colecao Volume.
    /// </sumario>
    public enum VolumeMes
    {
        Janeiro = 1,
        Fevereiro = 2,
        Marco = 3,
        Abril = 4,
        Maio = 5,
        Junho = 6,
        Julho = 7,
        Agosto = 8,
        Setembro = 9,
        Outubro = 10,
        Novembro = 11,
        Dezembro = 12
    }
}
