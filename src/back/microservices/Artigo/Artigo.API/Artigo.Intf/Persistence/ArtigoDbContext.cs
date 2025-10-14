using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Artigo.Intf.Enums;

namespace Artigo.Intf.PersistenceModels
{
    // --- Embedded Models ---

    /// <sumario>
    /// Objeto embutido para rastrear o papel do Autor em cada ciclo editorial.
    /// </sumario>
    public class ContribuicaoEditorialModel
    {
        public string ArtigoId { get; set; } = string.Empty;
        public ContribuicaoRole Role { get; set; }
    }

    /// <sumario>
    /// Objeto embutido para gerenciar a equipe de revisao e edicao.
    /// </sumario>
    public class EditorialTeamModel
    {
        public List<string> InitialAuthorId { get; set; } = [];
        public string EditorId { get; set; } = string.Empty;
        public List<string> ReviewerIds { get; set; } = [];
        public List<string> CorrectorIds { get; set; } = [];
    }

    // --- Core Collection Models ---

    /// <sumario>
    /// Modelo de Persistencia para a colecao Artigo.
    /// Contem o mapeamento Bson.
    /// </sumario>
    public class ArtigoModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        // Conteudo principal
        public string Titulo { get; set; } = string.Empty;
        public string Resumo { get; set; } = string.Empty;
        public ArtigoStatus Status { get; set; }
        public ArtigoTipo Tipo { get; set; }

        // Relacionamentos
        public List<string> AutorIds { get; set; } = [];
        public List<string> AutorReference { get; set; } = [];
        public string EditorialId { get; set; } = string.Empty;
        public string? VolumeId { get; set; }
        public List<string> MidiaIds { get; set; } = [];

        // Metricas Denormalizadas
        public int TotalInteracoes { get; set; } = 0;
        public int TotalComentarios { get; set; } = 0;

        // Datas
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataPublicacao { get; set; }
        public DateTime? DataEdicao { get; set; }
        public DateTime? DataAcademica { get; set; }
    }

    /// <sumario>
    /// Modelo de Persistencia para a colecao Autor.
    /// </sumario>
    public class AutorModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = string.Empty;
        public List<string> ArtigoWorkIds { get; set; } = [];
        public List<ContribuicaoEditorialModel> Contribuicoes { get; set; } = [];
    }

    /// <sumario>
    /// Modelo de Persistencia para a colecao Editorial.
    /// </sumario>
    public class EditorialModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string ArtigoId { get; set; } = string.Empty;
        public EditorialPosition Position { get; set; }
        public string CurrentHistoryId { get; set; } = string.Empty;
        public List<string> HistoryIds { get; set; } = [];
        public List<string> CommentIds { get; set; } = [];
        public EditorialTeamModel Team { get; set; } = new EditorialTeamModel();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <sumario>
    /// Modelo de Persistencia para a colecao ArtigoHistory.
    /// </sumario>
    public class ArtigoHistoryModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public ArtigoVersion Version { get; set; }
        public string ArtigoId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    /// <sumario>
    /// Modelo de Persistencia para a colecao Interaction.
    /// </sumario>
    public class InteractionModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string ArtigoId { get; set; } = string.Empty;
        // RENOMEADO: Usando ParentCommentId para consistencia com a entidade de Dominio.
        public string? ParentCommentId { get; set; } // ID do pai do comentario (ou null/string.Empty se for raiz) 
        public string UsuarioId { get; set; } = string.Empty; // ID do usuário que fez a interação
        public InteractionType Type { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }

    /// <sumario>
    /// Modelo de Persistencia para a colecao Staff.
    /// </sumario>
    public class StaffModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = string.Empty;
        public JobRole Job { get; set; }
    }

    /// <sumario>
    /// Modelo de Persistencia para a colecao Pending.
    /// </sumario>
    public class PendingModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public TargetEntityType TargetType { get; set; }
        public string TargetEntityId { get; set; } = string.Empty;
        public PendingStatus Status { get; set; } = PendingStatus.AwaitingReview;

        // RENOMEADOS para consistencia com a entidade de Dominio (Pending.cs)
        public DateTime DateRequested { get; set; } = DateTime.UtcNow;
        public string RequesterUsuarioId { get; set; } = string.Empty;
        public string Commentary { get; set; } = string.Empty;
        public string CommandParametersJson { get; set; } = string.Empty; // Antigo ComandoCMD, renomeado para consistencia
    }

    /// <sumario>
    /// Modelo de Persistencia para a colecao Volume.
    /// </sumario>
    public class VolumeModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public int Edicao { get; set; }
        public string VolumeTitulo { get; set; } = string.Empty;
        public string VolumeResumo { get; set; } = string.Empty;
        public VolumeMes M { get; set; }
        public int N { get; set; }
        public int Year { get; set; }
        public List<string> ArtigoIds { get; set; } = [];
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
