import { gql } from "@apollo/client";

export const GET_HOME_PAGE_DATA = gql`
  query GetHomePageData {
    latestArticles: obterArtigosCardList(pagina: 0, tamanho: 10) {
      id
      titulo
      resumo
      midiaDestaque {
        url
        textoAlternativo
      }
    }
    latestVolumes: obterVolumesList(pagina: 0, tamanho: 5) {
      id
      volumeTitulo
      volumeResumo
      imagemCapa {
        url
        textoAlternativo
      }
    }
  }
`;

export const GET_MEUS_ARTIGOS = gql`
  query GetMeusArtigos {
    obterMeusArtigosCardList {
      id
      titulo
      resumo
      status
      midiaDestaque {
        url
        textoAlternativo
      }
    }
  }
`;

export const SEARCH_ARTICLES = gql`
  query SearchArticles(
    $searchTerm: String!
    $searchByTitle: Boolean!
    $searchByAuthor: Boolean!
    $page: Int!
    $pageSize: Int!
  ) {
    titleResults: obterArtigoCardListPorTitulo(
      searchTerm: $searchTerm
      pagina: $page
      tamanho: $pageSize
    ) @include(if: $searchByTitle) {
      id
      titulo
      resumo
      status
      midiaDestaque {
        url
        textoAlternativo
      }
    }
    authorResults: obterArtigoCardListPorNomeAutor(
      searchTerm: $searchTerm
      pagina: $page
      tamanho: $pageSize
    ) @include(if: $searchByAuthor) {
      id
      titulo
      resumo
      status
      midiaDestaque {
        url
        textoAlternativo
      }
    }
  }
`;

export const GET_AUTOR_VIEW = gql`
  query GetAutorView($autorId: ID!) {
    obterAutorView(autorId: $autorId) {
      usuarioId
      nome
      url
      artigoWorkIds
    }
  }
`;

export const GET_ARTIGOS_BY_IDS = gql`
  query GetArtigosByIds($ids: [ID!]!) {
    obterArtigoCardListPorLista(ids: $ids) {
      id
      titulo
      resumo
      status
      midiaDestaque {
        url
        textoAlternativo
      }
    }
  }
`;

export const GET_ARTIGOS_POR_TIPO = gql`
  query GetArtigosPorTipo($tipo: TipoArtigo!, $page: Int!, $pageSize: Int!) {
    obterArtigosCardListPorTipo(tipo: $tipo, pagina: $page, tamanho: $pageSize) {
      id
      titulo
      resumo
      midiaDestaque {
        url
        textoAlternativo
      }
    }
  }
`;

export const GET_VOLUME_VIEW = gql`
  query GetVolumeView($volumeId: ID!) {
    obterVolumeView(volumeId: $volumeId) {
      id
      volumeTitulo
      volumeResumo
      imagemCapa {
        url
        textoAlternativo
      }
      artigos { 
        id
        titulo
        resumo
        midiaDestaque {
          url
          textoAlternativo
        }
      }
    }
  }
`;

export const VERIFICAR_STAFF = gql`
  query VerificarStaff {
    verificarStaff
  }
`;

const COMMENT_FIELDS = gql`
  fragment CommentFields on Interaction {
    id
    artigoId
    usuarioId
    usuarioNome
    content
    dataCriacao
    parentCommentId
    replies {
      id
      artigoId
      usuarioId
      usuarioNome
      content
      dataCriacao
      parentCommentId
    }
  }
`;

export const GET_ARTIGO_VIEW = gql`
  query GetArtigoView($artigoId: ID!) {
    obterArtigoView(artigoId: $artigoId) {
      id
      titulo
      tipo
      permitirComentario
      # (NOVO) Adiciona o total de comentários para o botão "Carregar Mais"
      totalComentarios: totalComentariosPublicos 
      midiaDestaque { 
        url
        textoAlternativo
      }
      conteudoAtual { 
        content
        midias {
          url
          textoAlternativo
        }
      }
      autores { 
        usuarioId
        nome
        url
      }
      volume { 
        id
        volumeTitulo
        volumeResumo
      }
      # (MODIFICADO) Busca APENAS comentários editoriais
      interacoes(page: 0, pageSize: 999) { # Pega todos os editoriais
        comentariosEditoriais {
          ...CommentFields
        }
      }
    }
  }
  ${COMMENT_FIELDS} 
`;

export const GET_COMENTARIOS_PUBLICOS = gql`
  query GetComentariosPublicos(
    $artigoId: ID!
    $page: Int!
    $pageSize: Int!
  ) {
    obterComentariosPublicos(
      artigoId: $artigoId
      pagina: $page
      tamanho: $pageSize
    ) {
      ...CommentFields
    }
  }
  ${COMMENT_FIELDS}
`;

export const CRIAR_COMENTARIO_PUBLICO = gql`
  mutation CriarComentarioPublico(
    $artigoId: ID!
    $content: String!
    $usuarioNome: String!
    $parentCommentId: ID
  ) {
    criarComentarioPublico(
      artigoId: $artigoId
      content: $content
      usuarioNome: $usuarioNome
      parentCommentId: $parentCommentId
    ) {
      ...CommentFields
    }
  }
  ${COMMENT_FIELDS}
`;

export const ATUALIZAR_INTERACAO = gql`
  mutation AtualizarInteracao(
    $interacaoId: ID!
    $newContent: String!
    $commentary: String!
  ) {
    atualizarInteracao(
      interacaoId: $interacaoId
      newContent: $newContent
      commentary: $commentary
    ) {
      id
      content
    }
  }
`;

export const DELETAR_INTERACAO = gql`
  mutation DeletarInteracao($interacaoId: ID!, $commentary: String!) {
    deletarInteracao(interacaoId: $interacaoId, commentary: $commentary)
  }
`;

export const CRIAR_ARTIGO = gql`
  mutation CriarArtigo($input: CreateArtigoInput!, $commentary: String!) {
    criarArtigo(input: $input, commentary: $commentary) {
      id
      titulo
      status
      # Retorna o editorial para redirecionamento ou validação futura
      editorial {
        id
      }
    }
  }
`;

