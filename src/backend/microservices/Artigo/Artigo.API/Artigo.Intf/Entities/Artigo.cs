using System;
using System.Collections.Generic;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa o Core da entidade Artigo no Dominio. 
    /// Contem o conteudo principal e referências simples a outras coleções.
    /// </sumario>
    public class Artigo
    {
        // Identificador do Dominio: Representado como uma string simples nesta camada.
        public string Id { get; set; } = string.Empty;

        // Conteudo do Core e Metadata
        public string Titulo { get; set; } = string.Empty;
        public string Resumo { get; set; } = string.Empty;

        // Status e ciclo de vida
        public StatusArtigo Status { get; set; } = StatusArtigo.Rascunho;
        public TipoArtigo Tipo { get; set; }

        // Relacionamento com outras coleções (guardadas como referencias ao UsuarioId)

        // Referência a coleção Autor (Autores involvidos na criacao)
        public List<string> AutorIds { get; set; } = [];

        // Lista somente pelo nome Autores que não são usuários cadastrados na plataforma
        public List<string> AutorReference { get; set; } = [];

        // Referência a coleção Editorial (Informações sobre o ciclo de vida editorial)
        public string EditorialId { get; set; } = string.Empty;

        // Referência a coleção Volume (Somente quando estiver publicado)
        public string? VolumeId { get; set; }

        // Armazena apenas a mídia de destaque (capa) para performance em listas.
        // A lista completa de mídias fica no ArtigoHistory.
        public MidiaEntry? MidiaDestaque { get; set; }

        // Metricas Denormalizadas (para uso em Padrões de Subset)
        public int TotalInteracoes { get; set; } = 0;
        public int TotalComentarios { get; set; } = 0;

        // Datas importantes

        // Data de criação do artigo
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // Data de publicação na revista
        public DateTime? DataPublicacao { get; set; }

        // Data da ultima modificacao editorial
        public DateTime? DataEdicao { get; set; }

        // Data de publicacao acadêmica quando o artigo já tiver sido publicado em outra revista
        public DateTime? DataAcademica { get; set; }

        // Controla se comentários públicos são permitidos neste artigo.
        public bool PermitirComentario { get; set; } = true;
    }

    /// <sumario>
    /// Objeto embutido para rastrear as informações de uma mídia associada ao Artigo.
    /// A posição no List<MidiaEntry> define se é a mídia de destaque (index 0).
    /// </sumario>
    public class MidiaEntry
    {
        public string MidiaID { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Alt { get; set; } = string.Empty; // Texto alternativo para SEO e acessibilidade
    }
}