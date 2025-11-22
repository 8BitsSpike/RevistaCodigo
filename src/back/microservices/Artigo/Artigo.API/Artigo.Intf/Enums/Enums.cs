namespace Artigo.Intf.Enums
{
    /// <sumario>
    /// Status do ciclo de vida editorial de um artigo.
    /// </sumario>
    public enum StatusArtigo
    {
        Rascunho,
        AguardandoAprovacao,
        EmRevisao,
        ProntoParaPublicar,
        Publicado,
        Arquivado
    }

    /// <sumario>
    /// Classificação do tipo de artigo (Artigo, Blog, Entrevista, Indicação e Opinião).
    /// </sumario>
    public enum TipoArtigo
    {
        Artigo,
        Blog,
        Entrevista,
        Indicacao,
        Opniao,
        Video,
        Administrativo
    }

    /// <sumario>
    /// Versões do corpo do artigo guardadas no histórico.
    /// Mapeia para o campo 'Version' na coleção ArtigoHistory.
    /// </sumario>
    public enum VersaoArtigo
    {
        Original = 0,
        PrimeiraEdicao = 1,
        SegundaEdicao = 2,
        TerceiraEdicao = 3,
        QuartaEdicao = 4,
        Final = 5 // Este indica que a versão está pronta para ser publicada
    }

    /// <sumario>
    /// Papel desempenhado por um Autor durante uma contribuição específica.
    /// </sumario>
    public enum FuncaoContribuicao
    {
        AutorPrincipal,
        Revisor,
        Corretor,
        EditorChefe
    }

    /// <sumario>
    /// Posição atual do Artigo no fluxo editorial.
    /// </sumario>
    public enum PosicaoEditorial
    {
        Submetido,              // Enviado pelo autor, aguardando triagem.
        AguardandoRevisao,      // Aguardando revisao pelos Revisores.
        RevisaoConcluida,       // Revisao concluída, aguardando correcao.
        AguardandoCorrecao,     // Aguardando correcao pelo Corretor.
        CorrecaoConcluida,      // Correcao concluída, aguardando redacao.
        AguardandoRedacao,      // Aguardando redacao final.
        RedacaoConcluida,       // Redacao concluída, pronto para a pauta.
        Rejeitado,              // Rejeitado em qualquer fase.
        EmEspera,               // Em espera de que algo aconteça.
        ProntoParaPublicar,     // Em condições para publicação.
        Publicado               // Publicado, oficialmente fora do ciclo editorial.
    }

    /// <sumario>
    /// Classifica o tipo de interação (comentário, like, etc.) implementada até o momento.
    /// </sumario>
    public enum TipoInteracao
    {
        ComentarioPublico,      // Comentário padrão de um usuário leitor.
        ComentarioEditorial     // Comentário feito por um membro da equipe editorial sobre o artigo em revisão.
    }

    /// <sumario>
    /// Status de uma requisição pendente.
    /// </sumario>
    public enum StatusPendente
    {
        AguardandoRevisao,      // Aguardando decisao de um editor ou administrador.
        Aprovado,               // Requisição aprovada e executada.
        Rejeitado,              // Requisição rejeitada.
        Arquivado               // Requisição antiga ou concluída.
    }

    /// <sumario>
    /// Tipo de entidade referenciada na requisição pendente.
    /// </sumario>
    public enum TipoEntidadeAlvo
    {
        Artigo,                 // Ação afeta o artigo.
        Autor,                  // Ação afeta o registro local do autor.
        Comentario,             // Ação afeta o comentário
        Staff,                  // Ação afeta o registro de staff (e.g., mudança de função).
        Volume,                 // Ação afeta a publicação (e.g., reordenar artigos em uma edição).
        Editorial               // Ação afeta a equipe editorial.
    }

    /// <sumario>
    /// Funcao de um membro da equipe (Staff) para fins de autorização.
    /// Define o nivel de permissão para aprovar Pendings ou executar ações criticas.
    /// </sumario>
    public enum FuncaoTrabalho
    {
        Administrador,         // Permissão total no sistema.
        EditorBolsista,        // Permissão para criar Pendings.
        EditorChefe,           // Permissão para gerenciar revisores e aprovar Pendings.
        Aposentado             // Membro da equipe inativo, mantido para histórico e referências.
    }

    /// <sumario>
    /// Mês de publicação para a coleção Volume.
    /// </sumario>
    public enum MesVolume
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

    /// <sumario>
    /// Status do ciclo de vida de um Volume (Edição).
    /// </sumario>
    public enum StatusVolume
    {
        EmRevisao,      // Em planejamento, não visível publicamente.
        Publicado,      // Visível publicamente.
        Arquivado       // Edição antiga, não visível na lista principal.
    }
}