export enum StatusArtigo {
  Rascunho = "RASCUNHO",
  AguardandoAprovacao = "AGUARDANDO_APROVACAO",
  EmRevisao = "EM_REVISAO",
  ProntoParaPublicar = "PRONTO_PARA_PUBLICAR",
  Publicado = "PUBLICADO",
  Arquivado = "ARQUIVADO",
}

export enum TipoArtigo {
  Artigo = "ARTIGO",
  Blog = "BLOG",
  Entrevista = "ENTREVISTA",
  Indicacao = "INDICACAO",
  // ATENÇÃO: Mantive "OPNIAO" (sem o i) para bater com o seu C#
  // Se você corrigir no C# para "Opiniao", altere aqui para "OPINIAO"
  Opniao = "OPNIAO", 
  Video = "VIDEO",
  Administrativo = "ADMINISTRATIVO",
}
export enum FuncaoTrabalho {
  Administrador = "ADMINISTRADOR",
  EditorBolsista = "EDITOR_BOLSISTA",
  EditorChefe = "EDITOR_CHEFE",
  Aposentado = "APOSENTADO"
}
export enum StatusVolume {
  EmRevisao = "EM_REVISAO",
  Publicado = "PUBLICADO",
  Arquivado = "ARQUIVADO",
}

export enum MesVolume {
  // O GraphQL geralmente espera o NOME em maiúsculo, não o número
  Janeiro = "JANEIRO",
  Fevereiro = "FEVEREIRO",
  Marco = "MARCO",
  Abril = "ABRIL",
  Maio = "MAIO",
  Junho = "JUNHO",
  Julho = "JULHO",
  Agosto = "AGOSTO",
  Setembro = "SETEMBRO",
  Outubro = "OUTUBRO",
  Novembro = "NOVEMBRO",
  Dezembro = "DEZEMBRO",
}

export enum PosicaoEditorial {
  Submetido = "SUBMETIDO",
  AguardandoRevisao = "AGUARDANDO_REVISAO",
  RevisaoConcluida = "REVISAO_CONCLUIDA",
  AguardandoCorrecao = "AGUARDANDO_CORRECAO",
  CorrecaoConcluida = "CORRECAO_CONCLUIDA",
  AguardandoRedacao = "AGUARDANDO_REDACAO",
  RedacaoConcluida = "REDACAO_CONCLUIDA",
  Rejeitado = "REJEITADO",
  EmEspera = "EM_ESPERA",
  ProntoParaPublicar = "PRONTO_PARA_PUBLICAR",
  Publicado = "PUBLICADO",
}

export enum VersaoArtigo {
  // Para Enums numéricos mapeados para valor, o GraphQL costuma tratar como string descritiva
  // Mas se o seu schema espera INT, mantenha os números.
  // SE DER ERRO AQUI, mude para: Original = "ORIGINAL", etc.
  Original = 0,
  PrimeiraEdicao = 1,
  SegundaEdicao = 2,
  TerceiraEdicao = 3,
  QuartaEdicao = 4,
  Final = 5,
}

// Adicionei os que faltavam baseados no seu C#
export enum StatusPendente {
  AguardandoRevisao = "AGUARDANDO_REVISAO",
  Aprovado = "APROVADO",
  Rejeitado = "REJEITADO",
  Arquivado = "ARQUIVADO",
}

export enum TipoEntidadeAlvo {
  Artigo = "ARTIGO",
  Autor = "AUTOR",
  Comentario = "COMENTARIO",
  Staff = "STAFF",
  Volume = "VOLUME",
  Editorial = "EDITORIAL",
}