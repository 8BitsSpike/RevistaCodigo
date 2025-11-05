using Artigo.DbContext.PersistenceModels;
using Artigo.Intf.Entities;
using AutoMapper;
using Microsoft.VisualBasic;
using SharpCompress.Common;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Artigo.DbContext.Mappers
{
    /// <sumario>
    /// Perfil de mapeamento AutoMapper para conversão entre Entidades de Dominio e Modelos de Persistencia (MongoDB).
    /// Este perfil garante que o Repositório não precise conter logica de mapeamento manual.
    /// </sumario>
    public class PersistenceMappingProfile : Profile
    {
        public PersistenceMappingProfile()
        {
            // Mapeamentos de Tipos Embutidos (Embedded Types)

            // ContribuicaoEditorial <-> ContribuicaoEditorialModel
            CreateMap<ContribuicaoEditorial, ContribuicaoEditorialModel>().ReverseMap();

            // EditorialTeam <-> EditorialTeamModel
            CreateMap<EditorialTeam, EditorialTeamModel>().ReverseMap();

            // MidiaEntry <-> MidiaEntryModel
            CreateMap<MidiaEntry, MidiaEntryModel>().ReverseMap();

            // *** NOVO MAPEAMENTO ***
            // StaffComentario <-> StaffComentarioModel
            CreateMap<StaffComentario, StaffComentarioModel>()
                // Garante que o ID seja mapeado com a representação BSON correta
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? MongoDB.Bson.ObjectId.GenerateNewId().ToString() : src.Id))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));


            // =================================================================================
            // Mapeamentos de Entidades de Colecoes (Collection Entities)
            // =================================================================================

            // Artigo <-> ArtigoModel
            CreateMap<Artigo.Intf.Entities.Artigo, ArtigoModel>()
                .ForMember(dest => dest.PermitirComentario, opt => opt.MapFrom(src => src.PermitirComentario)) // NOVO
                .ReverseMap();
            // Nota: O .ReverseMap() lidara com a conversão da lista Midias automaticamente.

            // Autor <-> AutorModel
            CreateMap<Autor, AutorModel>()
                // NOVOS CAMPOS
                .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                // FIM NOVOS CAMPOS
                .ForMember(dest => dest.Contribuicoes, opt => opt.MapFrom(src => src.Contribuicoes.Select(c => new ContribuicaoEditorialModel
                {
                    ArtigoId = c.ArtigoId,
                    Role = c.Role
                })));

            CreateMap<AutorModel, Autor>()
                .ForMember(dest => dest.Contribuicoes, opt => opt.MapFrom(src => src.Contribuicoes.Select(m => new ContribuicaoEditorial
                {
                    ArtigoId = m.ArtigoId,
                    Role = m.Role
                })));
            // Nota: Mapeamento explicito para a lista de objetos embutidos Contribuições.

            // Editorial <-> EditorialModel
            CreateMap<Editorial, EditorialModel>()
                // Mapeamento do objeto embutido Team (já mapeado acima)
                .ForMember(dest => dest.Team, opt => opt.MapFrom(src => src.Team));

            CreateMap<EditorialModel, Editorial>()
                .ForMember(dest => dest.Team, opt => opt.MapFrom(src => src.Team));

            // ArtigoHistory <-> ArtigoHistoryModel
            CreateMap<ArtigoHistory, ArtigoHistoryModel>()
                .ForMember(dest => dest.StaffComentarios, opt => opt.MapFrom(src => src.StaffComentarios)) // NOVO
                .ReverseMap();

            // Interaction <-> InteractionModel
            CreateMap<Artigo.Intf.Entities.Interaction, InteractionModel>()
                // Mapeamento do novo campo de desnormalização
                .ForMember(dest => dest.UsuarioNome, opt => opt.MapFrom(src => src.UsuarioNome))
                .ReverseMap();
            // Nota: ReverseMap cuida do mapeamento de InteractionModel para Interaction, incluindo UsuarioNome.


            // *** MAPEAMENTO ATUALIZADO (CORREÇÃO DO TESTE) ***
            // Pending <-> PendingModel
            CreateMap<Pending, PendingModel>()
                // Mapeamento explícito para todos os campos para garantir a correspondência
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TargetEntityId, opt => opt.MapFrom(src => src.TargetEntityId))
                .ForMember(dest => dest.TargetType, opt => opt.MapFrom(src => src.TargetType))
                .ForMember(dest => dest.RequesterUsuarioId, opt => opt.MapFrom(src => src.RequesterUsuarioId))
                .ForMember(dest => dest.CommandType, opt => opt.MapFrom(src => src.CommandType))
                .ForMember(dest => dest.Commentary, opt => opt.MapFrom(src => src.Commentary))
                .ForMember(dest => dest.CommandParametersJson, opt => opt.MapFrom(src => src.CommandParametersJson))
                .ForMember(dest => dest.DateRequested, opt => opt.MapFrom(src => src.DateRequested))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.IdAprovador, opt => opt.MapFrom(src => src.IdAprovador))
                .ForMember(dest => dest.DataAprovacao, opt => opt.MapFrom(src => src.DataAprovacao))
                .ReverseMap(); // O ReverseMap() fará a correspondência de volta


            // Staff <-> StaffModel
            CreateMap<Staff, StaffModel>()
                // NOVOS CAMPOS
                .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                // FIM NOVOS CAMPOS
                .ReverseMap();

            // Volume <-> VolumeModel
            CreateMap<Artigo.Intf.Entities.Volume, VolumeModel>()
                .ForMember(dest => dest.ImagemCapa, opt => opt.MapFrom(src => src.ImagemCapa)) // NOVO
                .ReverseMap();
        }
    }
}