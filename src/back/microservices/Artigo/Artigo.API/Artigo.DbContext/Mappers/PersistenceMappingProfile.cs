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
    /// Perfil de mapeamento AutoMapper para conversao entre Entidades de Dominio e Modelos de Persistencia (MongoDB).
    /// Este perfil garante que o Repositorio nao precise conter logica de mapeamento manual.
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

            // =================================================================================
            // Mapeamentos de Entidades de Colecoes (Collection Entities)
            // =================================================================================

            // Artigo <-> ArtigoModel
            CreateMap<Artigo.Intf.Entities.Artigo, ArtigoModel>().ReverseMap();
            // Nota: O .ReverseMap() lidara com a conversao de ObjectId (string) para string,
            // e os Enums padrao (ArtigoStatus, ArtigoTipo) que sao consistentes.

            // Autor <-> AutorModel
            CreateMap<Autor, AutorModel>()
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
            // Nota: Mapeamento explicito para a lista de objetos embutidos Contribuicoes.

            // Editorial <-> EditorialModel
            CreateMap<Editorial, EditorialModel>()
                // Mapeamento do objeto embutido Team (ja mapeado acima)
                .ForMember(dest => dest.Team, opt => opt.MapFrom(src => src.Team));

            CreateMap<EditorialModel, Editorial>()
                .ForMember(dest => dest.Team, opt => opt.MapFrom(src => src.Team));

            // ArtigoHistory <-> ArtigoHistoryModel
            CreateMap<ArtigoHistory, ArtigoHistoryModel>().ReverseMap();

            // Interaction <-> InteractionModel
            CreateMap<Artigo.Intf.Entities.Interaction, InteractionModel>().ReverseMap();

            // Pending <-> PendingModel
            CreateMap<Pending, PendingModel>().ReverseMap();

            // Staff <-> StaffModel
            CreateMap<Staff, StaffModel>().ReverseMap();

            // Volume <-> VolumeModel
            CreateMap<Artigo.Intf.Entities.Volume, VolumeModel>().ReverseMap();
        }
    }
}