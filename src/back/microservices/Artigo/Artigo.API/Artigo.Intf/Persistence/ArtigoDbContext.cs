using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Artigo.Intf.Enums; // Reference to Domain Enums

namespace Artigo.Intf.PersistenceModels
{
    // --- Embedded Models ---

    /// <sumario>
    /// Objeto embutido para rastrear o papel do Autor em cada ciclo editorial.
    /// </sumario>
    public class ContribuicaoEditorialModel
    {
        public string ArtigoId { get; set; } = string.Empty;
        public FuncaoContribuicao Role { get; set; } // CORRIGIDO: FuncaoContribuicao
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

    /// <sumario>
    /// Objeto embutido para rastrear as informações de uma mídia associada ao Artigo.
    /// </sumario>
    public class MidiaEntryModel
    {
        public string MidiaID { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Alt { get; set; } = string.Empty; // Texto alternativo
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
        public StatusArtigo Status { get; set; } // CORRIGIDO: StatusArtigo
        public TipoArtigo Tipo { get; set; } // CORRIGIDO: TipoArtigo

        // Relacionamentos
        public List<string> AutorIds { get; set; } = [];
        public List<string> AutorReference { get; set; } = [];
        public string EditorialId { get; set; } = string.Empty;
        public string? VolumeId { get; set; }
        public List<MidiaEntryModel> Midias { get; set; } = [];

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
        public PosicaoEditorial Position { get; set; } // CORRIGIDO: PosicaoEditorial
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

        public VersaoArtigo Version { get; set; } // CORRIGIDO: VersaoArtigo
        public string ArtigoId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<MidiaEntryModel> Midias { get; set; } = [];
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
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
        public string? ParentCommentId { get; set; }
        public string UsuarioId { get; set; } = string.Empty;

        // NOVO: Nome de exibição do usuário (obtido do UsuarioAPI no momento da criação).
        public string UsuarioNome { get; set; } = string.Empty;

        public TipoInteracao Type { get; set; } // CORRIGIDO: TipoInteracao
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
        public FuncaoTrabalho Job { get; set; } // CORRIGIDO: FuncaoTrabalho
    }

    /// <sumario>
    /// Modelo de Persistencia para a colecao Pending.
    /// </sumario>
    public class PendingModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public TipoEntidadeAlvo TargetType { get; set; } // CORRIGIDO: TipoEntidadeAlvo
        public string TargetEntityId { get; set; } = string.Empty;
        public StatusPendente Status { get; set; } = StatusPendente.AguardandoRevisao; // CORRIGIDO: StatusPendente

        public DateTime DateRequested { get; set; } = DateTime.UtcNow;
        public string RequesterUsuarioId { get; set; } = string.Empty;
        public string Commentary { get; set; } = string.Empty;
        public string CommandParametersJson { get; set; } = string.Empty;

        // NOVO: ID do usuário Staff que aprovou/rejeitou o pedido (UsuarioId externo).
        public string? IdAprovador { get; set; }

        // NOVO: Data e hora em que o pedido foi resolvido (aprovado ou rejeitado).
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
        public MesVolume M { get; set; } // CORRIGIDO: MesVolume
        public int N { get; set; }
        public int Year { get; set; }
        public List<string> ArtigoIds { get; set; } = [];
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}