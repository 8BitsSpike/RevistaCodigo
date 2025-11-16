export enum StatusArtigo {
    Rascunho = "Rascunho",
    AguardandoAprovacao = "AguardandoAprovacao",
    EmRevisao = "EmRevisao",
    ProntoParaPublicar = "ProntoParaPublicar",
    Publicado = "Publicado",
    Arquivado = "Arquivado",
}

export enum TipoArtigo {
    Artigo = "Artigo",
    Blog = "Blog",
    Entrevista = "Entrevista",
    Indicacao = "Indicacao",
    Opniao = "Opniao",
    Administrativo = "Administrativo",
}

export enum StatusVolume {
    EmRevisao = "EmRevisao",
    Publicado = "Publicado",
    Arquivado = "Arquivado",
}

export enum MesVolume {
    Janeiro = "Janeiro",
    Fevereiro = "Fevereiro",
    Marco = "Marco",
    Abril = "Abril",
    Maio = "Maio",
    Junho = "Junho",
    Julho = "Julho",
    Agosto = "Agosto",
    Setembro = "Setembro",
    Outubro = "Outubro",
    Novembro = "Novembro",
    Dezembro = "Dezembro",
}

export enum PosicaoEditorial {
    Submetido = "Submetido",
    AguardandoRevisao = "AguardandoRevisao",
    RevisaoConcluida = "RevisaoConcluida",
    AguardandoCorrecao = "AguardandoCorrecao",
    CorrecaoConcluida = "CorrecaoConcluida",
    AguardandoRedacao = "AguardandoRedacao",
    RedacaoConcluida = "RedacaoConcluida",
    Rejeitado = "Rejeitado",
    EmEspera = "EmEspera",
    ProntoParaPublicar = "ProntoParaPublicar",
    Publicado = "Publicado",
}

export enum VersaoArtigo {
    Original = 0,
    PrimeiraEdicao = 1,
    SegundaEdicao = 2,
    TerceiraEdicao = 3,
    QuartaEdicao = 4,
    Final = 5,
}