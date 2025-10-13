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
            // --- Conversao de Saída: Entidade (Domain) para DTO (Publico) ---
            CreateMap<Artigo.Intf.Entities.Artigo, ArtigoDTO>()
                // Os Enums sao mapeados para string por padrão, mas explicitamos para clareza
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo.ToString()));

            // --- Conversao para Updates: DTO (Saída) para Entidade (Domain) ---
            // Usado para atualizar entidades no servico a partir dos dados recebidos.
            CreateMap<ArtigoDTO, Artigo.Intf.Entities.Artigo>()
                // Ignoramos o ID no mapeamento para evitar que a entidade seja recriada acidentalmente.
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // --- Conversao de Entrada: CreateRequest DTO para Entidade (Domain) ---
            CreateMap<CreateArtigoRequest, Artigo.Intf.Entities.Artigo>()
                // Ignora propriedades que serao definidas pela camada de servico/repositorio
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ArtigoStatus.Draft)) // Status inicial
                .ForMember(dest => dest.DataCriacao, opt => opt.MapFrom(src => DateTime.UtcNow)) // Define a data de criacao no mapeamento
                .ForMember(dest => dest.EditorialId, opt => opt.Ignore())
                .ForMember(dest => dest.VolumeId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalInteracoes, opt => opt.Ignore())
                .ForMember(dest => dest.TotalComentarios, opt => opt.Ignore())
                .ForMember(dest => dest.DataPublicacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataEdicao, opt => opt.Ignore())
                .ForMember(dest => dest.DataAcademica, opt => opt.Ignore());

            // --- Mapeamentos de Tipos Customizados (String <-> Enum) ---

            // Map String para Enums
            CreateMap<string, ArtigoStatus>().ConvertUsing(src => Enum.Parse<ArtigoStatus>(src));
            CreateMap<string, ArtigoTipo>().ConvertUsing(src => Enum.Parse<ArtigoTipo>(src));

            // Map Enums para String (necessario para flexibilidade, embora o default funcione)
            CreateMap<ArtigoStatus, string>().ConvertUsing(src => src.ToString());
            CreateMap<ArtigoTipo, string>().ConvertUsing(src => src.ToString());

        }
    }
}
