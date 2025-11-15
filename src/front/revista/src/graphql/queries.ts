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

// Esta é a query de busca PÚBLICA (para a página /search)
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
      status # Adicionado após a atualização do DTO
      tipo # Adicionado após a atualização do DTO
      permitirComentario # Adicionado após a atualização do DTO
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
      status # Adicionado após a atualização do DTO
      tipo # Adicionado após a atualização do DTO
      permitirComentario # Adicionado após a atualização do DTO
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
      tipo
      permitirComentario
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
      status
      tipo
      permitirComentario
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

// --- Fragmento e Queries da Página de Artigo ---

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
      interacoes(page: 0, pageSize: 999) { 
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

// --- Mutações de Comentário ---

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

// --- Mutação de Submissão de Artigo ---

export const CRIAR_ARTIGO = gql`
  mutation CriarArtigo($input: CreateArtigoInput!, $commentary: String!) {
    criarArtigo(input: $input, commentary: $commentary) {
      id
      titulo
      status
      editorial {
        id
      }
    }
  }
`;

// --- Queries e Mutações da Sala Editorial ---

export const OBTER_STAFF_LIST = gql`
  query ObterStaffList($page: Int!, $pageSize: Int!) {
    obterStaffList(pagina: $page, tamanho: $pageSize) {
      id 
      usuarioId
      nome
      url
      job
      isActive
    }
  }
`;

export const CRIAR_NOVO_STAFF = gql`
  mutation CriarNovoStaff($input: CreateStaffInput!, $commentary: String!) {
    criarNovoStaff(input: $input, commentary: $commentary) {
      id
      usuarioId
      nome
      job
      isActive
    }
  }
`;

export const ATUALIZAR_STAFF = gql`
  mutation AtualizarStaff(
    $input: UpdateStaffInput!
    $commentary: String!
  ) {
    atualizarStaff(
      input: $input
      commentary: $commentary
    ) {
      id
      usuarioId
      job
      isActive
    }
  }
`;

export const OBTER_PENDENTES = gql`
  query ObterPendentes(
    $pagina: Int!
    $tamanho: Int!
    $status: StatusPendente
    $targetEntityId: ID
    $targetType: TipoEntidadeAlvo
    $requesterUsuarioId: ID
  ) {
    obterPendentes(
      pagina: $pagina
      tamanho: $tamanho
      status: $status
      targetEntityId: $targetEntityId
      targetType: $targetType
      requesterUsuarioId: $requesterUsuarioId
    ) {
      id
      targetEntityId
      targetType
      status
      dateRequested
      requesterUsuarioId
      commentary
      commandType
      commandParametersJson
      idAprovador
      dataAprovacao
    }
  }
`;

export const RESOLVER_REQUISICAO_PENDENTE = gql`
  mutation ResolverRequisicaoPendente(
    $pendingId: ID!
    $isApproved: Boolean!
  ) {
    resolverRequisicaoPendente(
      pendingId: $pendingId
      isApproved: $isApproved
    )
  }
`;

const EDITORIAL_CARD_FIELDS = gql`
  fragment EditorialCardFields on ArtigoCardList {
    id
    titulo
    resumo
    status
    tipo
    permitirComentario
    midiaDestaque {
      url
      textoAlternativo
    }
  }
`;

export const OBTER_ARTIGOS_POR_STATUS = gql`
  query ObterArtigosPorStatus(
    $status: StatusArtigo!
    $pagina: Int!
    $tamanho: Int!
  ) {
    obterArtigosPorStatus(
      status: $status
      pagina: $pagina
      tamanho: $tamanho
    ) {
      ...EditorialCardFields
    }
  }
  ${EDITORIAL_CARD_FIELDS}
`;

// (NOVA)
export const OBTER_ARTIGOS_EDITORIAL_POR_TIPO = gql`
  query ObterArtigosEditorialPorTipo(
    $tipo: TipoArtigo!
    $pagina: Int!
    $tamanho: Int!
  ) {
    obterArtigosEditorialPorTipo(
      tipo: $tipo
      pagina: $pagina
      tamanho: $tamanho
    ) {
      ...EditorialCardFields
    }
  }
  ${EDITORIAL_CARD_FIELDS}
`;

// (NOVA)
export const SEARCH_ARTIGOS_EDITORIAL_BY_TITLE = gql`
  query SearchArtigosEditorialByTitle(
    $searchTerm: String!
    $pagina: Int!
    $tamanho: Int!
  ) {
    searchArtigosEditorialByTitle(
      searchTerm: $searchTerm
      pagina: $pagina
      tamanho: $tamanho
    ) {
      ...EditorialCardFields
    }
  }
  ${EDITORIAL_CARD_FIELDS}
`;

// (NOVA)
export const SEARCH_ARTIGOS_EDITORIAL_BY_AUTOR_IDS = gql`
  query SearchArtigosEditorialByAutorIds(
    $idsAutor: [ID!]!
    $pagina: Int!
    $tamanho: Int!
  ) {
    searchArtigosEditorialByAutorIds(
      idsAutor: $idsAutor
      pagina: $pagina
      tamanho: $tamanho
    ) {
      ...EditorialCardFields
    }
  }
  ${EDITORIAL_CARD_FIELDS}
`;

export const ATUALIZAR_METADADOS_ARTIGO = gql`
  mutation AtualizarMetadadosArtigo(
    $id: ID!
    $input: UpdateArtigoInput!
    $commentary: String!
  ) {
    atualizarMetadadosArtigo(id: $id, input: $input, commentary: $commentary) {
      id
      status
      tipo
      permitirComentario
    }
  }
`;

const VOLUME_CARD_FIELDS = gql`
  fragment VolumeCardFields on VolumeCard {
    id
    volumeTitulo
    volumeResumo
    imagemCapa {
      url
      textoAlternativo
    }
  }
`;

// Query para buscar volumes recentes (formato card)
export const OBTER_VOLUMES = gql`
  query ObterVolumes($pagina: Int!, $tamanho: Int!) {
    obterVolumes(pagina: $pagina, tamanho: $tamanho) {
      ...VolumeCardFields
    }
  }
  ${VOLUME_CARD_FIELDS}
`;

// Query para buscar volumes por status (formato card)
export const OBTER_VOLUMES_POR_STATUS = gql`
  query ObterVolumesPorStatus(
    $status: StatusVolume!
    $pagina: Int!
    $tamanho: Int!
  ) {
    obterVolumesPorStatus(
      status: $status
      pagina: $pagina
      tamanho: $tamanho
    ) {
      ...VolumeCardFields
    }
  }
  ${VOLUME_CARD_FIELDS}
`;

// Query para buscar volumes por ano (retorna o tipo Volume completo)
export const OBTER_VOLUMES_POR_ANO = gql`
  query ObterVolumesPorAno($ano: Int!, $pagina: Int!, $tamanho: Int!) {
    obterVolumesPorAno(ano: $ano, pagina: $pagina, tamanho: $tamanho) {
      id
      volumeTitulo
      volumeResumo
      imagemCapa {
        url
        textoAlternativo
      }
      # Nota: Esta query na verdade retorna o tipo 'Volume' completo,
      # mas para o card, só precisamos destes campos.
    }
  }
`;

// Query para obter UM volume pelo ID (query de Staff)
export const OBTER_VOLUME_POR_ID = gql`
  query ObterVolumePorId($idVolume: ID!) {
    obterVolumePorId(idVolume: $idVolume) {
      id
      edicao
      volumeTitulo
      volumeResumo
      m
      n
      year
      status
      imagemCapa {
        midiaID
        url
        alt
      }
      artigoIds
    }
  }
`;

// Mutação para criar um novo volume
export const CRIAR_VOLUME = gql`
  mutation CriarVolume($input: CreateVolumeInputType!, $commentary: String!) {
    criarVolume(input: $input, commentary: $commentary) {
      id
      volumeTitulo
      status
    }
  }
`;

// Mutação para atualizar um volume
export const ATUALIZAR_METADADOS_VOLUME = gql`
  mutation AtualizarMetadadosVolume(
    $volumeId: ID!
    $input: UpdateVolumeMetadataInputType!
    $commentary: String!
  ) {
    atualizarMetadadosVolume(
      volumeId: $volumeId
      input: $input
      commentary: $commentary
    )
  }
`;

