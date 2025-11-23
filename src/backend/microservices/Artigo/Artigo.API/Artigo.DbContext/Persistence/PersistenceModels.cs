using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Artigo.Intf.Enums;

namespace Artigo.DbContext.PersistenceModels
{
    // --- Modelos embutido ---

    /// <sumario>
    /// Objeto embutido para rastrear o papel do Autor em cada ciclo editorial.
    /// </sumario>
    public class ContribuicaoEditorialModel
    {
        public string ArtigoId { get; set; } = string.Empty;
        public FuncaoContribuicao Role { get; set; }
    }

    /// <sumario>
    /// Objeto embutido para gerenciar a equipe de revisao e edicao.
    /// </sumario>
    public class EditorialTeamModel
    {
        public List<string> InitialAuthorId { get; set; } = [];

        public List<string> EditorIds { get; set; } = [];
        public List<string> ReviewerIds { get; set; } = [];
        public List<string> CorrectorIds { get; set; } = [];
    }

    /// <sumario>
    /// Objeto embutido para rastrear as informações de uma mídia associada ao Artigo.
    /// </sumario>
    public class MidiaEntryModel
    {
        public string MidiaID { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
        public string Alt { get; set; } = string.Empty; // Texto alternativo para inclusividade
    }

    /// <sumario>
    /// Objeto embutido para rastrear os comentários internos da equipe editorial
    /// sobre uma versão específica do ArtigoHistory.
    /// </sumario>
    [BsonIgnoreExtraElements] 
    public class StaffComentarioModel
    {
        [BsonElement("Id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = string.Empty;
        public DateTime Data { get; set; } = DateTime.UtcNow;
        public string? Parent { get; set; }
        public string Comment { get; set; } = string.Empty;
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

        [BsonRepresentation(BsonType.String)]
        public StatusArtigo Status { get; set; }
        [BsonRepresentation(BsonType.String)]
        public TipoArtigo Tipo { get; set; }

        // Relacionamentos
        public List<string> AutorIds { get; set; } = [];
        public List<string> AutorReference { get; set; } = [];
        public string EditorialId { get; set; } = string.Empty;
        public string? VolumeId { get; set; }

        // Armazena apenas a mídia de destaque (capa) para performance em listas.
        public MidiaEntryModel? MidiaDestaque { get; set; }

        // Metricas Denormalizadas
        public int TotalInteracoes { get; set; } = 0;
        public int TotalComentarios { get; set; } = 0;

        // Datas
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataPublicacao { get; set; }
        public DateTime? DataEdicao { get; set; }
        public DateTime? DataAcademica { get; set; }
        public bool PermitirComentario { get; set; } = true;
    }

    /// <sumario>
    /// Modelo de Persistencia para a coleção Autor.
    /// </sumario>
    public class AutorModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public List<string> ArtigoWorkIds { get; set; } = [];
        public List<ContribuicaoEditorialModel> Contribuicoes { get; set; } = [];
    }

    /// <sumario>
    /// Modelo de Persistencia para a coleção Editorial.
    /// </sumario>
    public class EditorialModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string ArtigoId { get; set; } = string.Empty;
        public PosicaoEditorial Position { get; set; }
        public string CurrentHistoryId { get; set; } = string.Empty;
        public List<string> HistoryIds { get; set; } = [];
        public List<string> CommentIds { get; set; } = [];
        public EditorialTeamModel Team { get; set; } = new EditorialTeamModel();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <sumario>
    /// Modelo de Persistencia para a coleção ArtigoHistory.
    /// </sumario>
    public class ArtigoHistoryModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public VersaoArtigo Version { get; set; }
        public string ArtigoId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<MidiaEntryModel> Midias { get; set; } = [];
        public List<StaffComentarioModel> StaffComentarios { get; set; } = [];
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    }

    /// <sumario>
    /// Modelo de Persistencia para a colecao Interaction.
    /// </sumario>
    /// 
    [BsonIgnoreExtraElements]
    public class InteractionModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string ArtigoId { get; set; } = string.Empty;
        public string? ParentCommentId { get; set; }
        public string UsuarioId { get; set; } = string.Empty;

        // Nome de exibição do usuário (obtido do UsuarioAPI no momento da criação).
        public string UsuarioNome { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public TipoInteracao Type { get; set; }
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
        public string Nome { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public FuncaoTrabalho Job { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <sumario>
    /// Modelo de Persistencia para a colecao Pending.
    /// </sumario>
    public class PendingModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public TipoEntidadeAlvo TargetType { get; set; }
        public string TargetEntityId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public StatusPendente Status { get; set; } = StatusPendente.AguardandoRevisao;
        public DateTime DateRequested { get; set; } = DateTime.UtcNow;
        public string RequesterUsuarioId { get; set; } = string.Empty;
        public string Commentary { get; set; } = string.Empty;
        public string CommandType { get; set; } = string.Empty;
        public string CommandParametersJson { get; set; } = string.Empty;

        // ID do usuário Staff que aprovou/rejeitou o pedido (UsuarioId externo).
        public string? IdAprovador { get; set; }

        // Data e hora em que o pedido foi resolvido (aprovado ou rejeitado).
        public DateTime? DataAprovacao { get; set; }
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
        public MesVolume M { get; set; }
        public int N { get; set; }
        public int Year { get; set; }

        [BsonRepresentation(BsonType.String)] // Salva o enum como string
        public StatusVolume Status { get; set; } = StatusVolume.EmRevisao;
        public MidiaEntryModel? ImagemCapa { get; set; }
        public List<string> ArtigoIds { get; set; } = [];
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}