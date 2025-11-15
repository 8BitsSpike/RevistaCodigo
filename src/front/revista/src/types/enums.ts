// Em types/enums.ts

/**
 * Status do ciclo de vida editorial de um artigo.
 */
export enum StatusArtigo {
    Rascunho = "Rascunho",
    AguardandoAprovacao = "AguardandoAprovacao",
    EmRevisao = "EmRevisao",
    ProntoParaPublicar = "ProntoParaPublicar",
    Publicado = "Publicado",
    Arquivado = "Arquivado",
}

/**
 * Classificação do tipo de artigo.
 */
export enum TipoArtigo {
    Artigo = "Artigo",
    Blog = "Blog",
    Entrevista = "Entrevista",
    Indicacao = "Indicacao",
    Opniao = "Opniao",
    Administrativo = "Administrativo",
}

/**
 * Status do ciclo de vida de um volume (edição).
 * Espelha o C# Enum: Artigo.Intf.Enums.StatusVolume
 */
export enum StatusVolume {
    EmRevisao = "EmRevisao",
    Publicado = "Publicado",
    Arquivado = "Arquivado",
}

/**
 * Mês de publicação para um volume.
 * Espelha o C# Enum: Artigo.Intf.Enums.MesVolume
 * (Nota: O GraphQL usa os Nomes, não valores numéricos)
 */
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