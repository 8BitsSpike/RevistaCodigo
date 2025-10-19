using System;
using System.Collections.Generic;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa o Core da entidade Artigo no Dominio. 
    /// Contem o conteudo principal e referencias simples a outras colecoes.
    /// </sumario>
    public class Artigo
    {
        // Identificador do Dominio: Representado como uma string simples nesta camada.
        public string Id { get; set; } = string.Empty;

        // Conteudo do Core e Metadata
        public string Titulo { get; set; } = string.Empty;
        public string Resumo { get; set; } = string.Empty;

        // Status e ciclo de vida
        public StatusArtigo Status { get; set; } = StatusArtigo.Rascunho; // CORRIGIDO: StatusArtigo.Rascunho
        public TipoArtigo Tipo { get; set; } // CORRIGIDO: TipoArtigo

        // Relacionamento com outras colecoes (guardadas como referencias ao UsuarioId)
        // Referencia a colecao Autor (Autores involvidos na criacao)
        public List<string> AutorIds { get; set; } = [];

        // Lista somente pelo nome Autores que nao sao usuarios cadastrados na plataforma
        public List<string> AutorReference { get; set; } = [];

        // Referencia a colecao Editorial (Informacoes sobre o ciclo de vida editorial)
        public string EditorialId { get; set; } = string.Empty;

        // Referencia a colecao Volume (Somente quando estiver publicado)
        public string? VolumeId { get; set; }

        // MUDANÇA: Lista de objetos Midia (já alterado)
        public List<MidiaEntry> Midias { get; set; } = []; // ADICIONADO

        // Metricas Denormalizadas (para uso em Padroes de Subset)
        public int TotalInteracoes { get; set; } = 0;
        public int TotalComentarios { get; set; } = 0;

        // Datas importantes
        // Data de criacao do artigo
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // Data de publicacao na revista
        public DateTime? DataPublicacao { get; set; }

        // Data da ultima modificacao editorial
        public DateTime? DataEdicao { get; set; }

        // Data de publicacao academica quando o artico ja tiver sido publicado em outra revista
        public DateTime? DataAcademica { get; set; }
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