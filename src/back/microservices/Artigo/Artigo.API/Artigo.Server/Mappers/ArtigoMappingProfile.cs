using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs;
using AutoMapper;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Artigo.Server.Mappers
{
    /// <sumario>
    /// Perfil de mapeamento AutoMapper para conversao entre entidades de Dominio e DTOs.
    /// Define as regras de conversao para evitar que a logica de negocio seja contaminada por mapeamento.
    /// </sumario>
    public class ArtigoMappingProfile : Profile
    {
        public ArtigoMappingProfile()
        {
            // =========================================================================
            // Mapeamentos de Tipos Embutidos (Embedded Types)
            // =========================================================================

            // ADICIONADO: MidiaEntry (Domain) <-> MidiaEntryDTO (Application)
            CreateMap<MidiaEntry, MidiaEntryDTO>()
                // Mapeamento explícito para nomes traduzidos no DTO
                .ForMember(dest => dest.IdMidia, opt => opt.MapFrom(src => src.MidiaID))
                .ForMember(dest => dest.TextoAlternativo, opt => opt.MapFrom(src => src.Alt))
                .ReverseMap()
                // Mapeamento reverso explícito (DTO -> Domain)
                .ForMember(dest => dest.MidiaID, opt => opt.MapFrom(src => src.IdMidia))
                .ForMember(dest => dest.Alt, opt => opt.MapFrom(src => src.TextoAlternativo));


            // =========================================================================
            // Mapeamentos de Entidades/DTOs
            // =========================================================================

            // --- Conversao de Saída: Entidade (Domain) para DTO (Publico) ---
            CreateMap<Artigo.Intf.Entities.Artigo, ArtigoDTO>()
                // Mapeamento explícito para nomes traduzidos no DTO
                .ForMember(dest => dest.IdsAutor, opt => opt.MapFrom(src => src.AutorIds))
                .ForMember(dest => dest.ReferenciasAutor, opt => opt.MapFrom(src => src.AutorReference))
                .ForMember(dest => dest.IdEditorial, opt => opt.MapFrom(src => src.EditorialId))
                .ForMember(dest => dest.IdVolume, opt => opt.MapFrom(src => src.VolumeId))

                // Os Enums sao mapeados para string por padrão, mas explicitamos para clareza
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo.ToString()));

            // --- Conversao para Updates: DTO (Saída) para Entidade (Domain) ---
            // Usado para atualizar entidades no servico a partir dos dados recebidos.
            CreateMap<ArtigoDTO, Artigo.Intf.Entities.Artigo>()
                // Mapeamento explícito para nomes traduzidos (DTO -> Domain)
                .ForMember(dest => dest.AutorIds, opt => opt.MapFrom(src => src.IdsAutor))
                .ForMember(dest => dest.AutorReference, opt => opt.MapFrom(src => src.ReferenciasAutor))
                .ForMember(dest => dest.EditorialId, opt => opt.MapFrom(src => src.IdEditorial))
                .ForMember(dest => dest.VolumeId, opt => opt.MapFrom(src => src.IdVolume))

                // Ignoramos o ID no mapeamento para evitar que a entidade seja recriada acidentalmente.
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // --- Conversao de Entrada: CreateRequest DTO para Entidade (Domain) ---
            CreateMap<CreateArtigoRequest, Artigo.Intf.Entities.Artigo>()
                // Mapeamento explícito para Autores (CORRIGIDO PARA NOVOS NOMES DO DTO)
                .ForMember(dest => dest.AutorIds, opt => opt.MapFrom(src => src.IdsAutor)) // Usando IdsAutor
                .ForMember(dest => dest.AutorReference, opt => opt.MapFrom(src => src.ReferenciasAutor)) // Usando ReferenciasAutor

                // NOVO: Ignora a lista de Midias, pois o DTO de criação não a contém.
                .ForMember(dest => dest.Midias, opt => opt.Ignore())

                // Ignora propriedades que serao definidas pela camada de servico/repositorio
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => StatusArtigo.Rascunho))
                .ForMember(dest => dest.DataCriacao, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.EditorialId, opt => opt.Ignore())
                .ForMember(dest => dest.VolumeId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalInteracoes, opt => opt.Ignore())
                .ForMember(dest => dest.TotalComentarios, opt => opt.Ignore())
                .ForMember(dest => dest.DataPublicacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataEdicao, opt => opt.Ignore())
                .ForMember(dest => dest.DataAcademica, opt => opt.Ignore());

            // --- Mapeamentos de Tipos Customizados (String <-> Enum) ---

            // Map String para Enums
            CreateMap<string, StatusArtigo>().ConvertUsing(src => Enum.Parse<StatusArtigo>(src));
            CreateMap<string, TipoArtigo>().ConvertUsing(src => Enum.Parse<TipoArtigo>(src));

            // Map Enums para String (necessario para flexibilidade, embora o default funcione)
            CreateMap<StatusArtigo, string>().ConvertUsing(src => src.ToString());
            CreateMap<TipoArtigo, string>().ConvertUsing(src => src.ToString());

        }
    }
}