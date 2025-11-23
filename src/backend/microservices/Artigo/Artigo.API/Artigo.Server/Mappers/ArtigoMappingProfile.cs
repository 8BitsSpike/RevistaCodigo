using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs;
using AutoMapper;
using System;
using System.Linq; // Adicionado para .Select() e .FirstOrDefault()
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Artigo.Server.Mappers
{
    /// <sumario>
    /// Perfil de mapeamento AutoMapper para conversão entre entidades de Dominio e DTOs.
    /// Define as regras de conversão para evitar que a logica de negócio seja contaminada por mapeamento.
    /// </sumario>
    public class ArtigoMappingProfile : Profile
    {
        public ArtigoMappingProfile()
        {
            // =========================================================================
            // Mapeamentos de Tipos Embutidos (Embedded Types)
            // =========================================================================

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
                .ForMember(dest => dest.EditorialId, opt => opt.MapFrom(src => src.EditorialId))
                .ForMember(dest => dest.IdVolume, opt => opt.MapFrom(src => src.VolumeId))
                .ForMember(dest => dest.PermitirComentario, opt => opt.MapFrom(src => src.PermitirComentario))

                // Ignora Midias, pois será um resolver no ArtigoType
                .ForMember(dest => dest.Midias, opt => opt.Ignore())

                // Os Enums sao mapeados para string por padrão, mas explicitamos para clareza
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status)) // Mapeamento direto de Enum para Enum
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo)); // Mapeamento direto de Enum para Enum

            // --- Conversão para Updates: DTO (Saída) para Entidade (Domain) ---
            // Usado para atualizar entidades no servico a partir dos dados recebidos.
            CreateMap<ArtigoDTO, Artigo.Intf.Entities.Artigo>()
                // Mapeamento explícito para nomes traduzidos (DTO -> Domain)
                .ForMember(dest => dest.AutorIds, opt => opt.MapFrom(src => src.IdsAutor))
                .ForMember(dest => dest.AutorReference, opt => opt.MapFrom(src => src.ReferenciasAutor))
                .ForMember(dest => dest.EditorialId, opt => opt.MapFrom(src => src.EditorialId))
                .ForMember(dest => dest.VolumeId, opt => opt.MapFrom(src => src.IdVolume))
                .ForMember(dest => dest.PermitirComentario, opt => opt.MapFrom(src => src.PermitirComentario))
                .ForMember(dest => dest.MidiaDestaque, opt => opt.Ignore()) // MidiaDestaque é gerenciado pelo serviço

                // Ignoramos o ID no mapeamento para evitar que a entidade seja recriada acidentalmente.
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // --- Conversao de Entrada: CreateRequest DTO para Entidade (Domain) ---
            CreateMap<CreateArtigoRequest, Artigo.Intf.Entities.Artigo>()
                // Mapeia a lista de AutorInputDTO para a lista de strings AutorIds
                .ForMember(dest => dest.AutorIds, opt => opt.MapFrom(src => src.Autores.Select(a => a.UsuarioId).ToList()))
                .ForMember(dest => dest.AutorReference, opt => opt.MapFrom(src => src.ReferenciasAutor))

                // Mapeia a *primeira* mídia para MidiaDestaque
                .ForMember(dest => dest.MidiaDestaque, opt => opt.MapFrom(src => src.Midias.FirstOrDefault()))

                // Ignora propriedades a serem definidas pela camada de servico/repositório
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => StatusArtigo.Rascunho))
                .ForMember(dest => dest.DataCriacao, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.EditorialId, opt => opt.Ignore())
                .ForMember(dest => dest.VolumeId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalInteracoes, opt => opt.Ignore())
                .ForMember(dest => dest.TotalComentarios, opt => opt.Ignore())
                .ForMember(dest => dest.DataPublicacao, opt => opt.Ignore())
                .ForMember(dest => dest.DataEdicao, opt => opt.Ignore())
                .ForMember(dest => dest.DataAcademica, opt => opt.Ignore())
                .ForMember(dest => dest.PermitirComentario, opt => opt.MapFrom(src => true)); // Default value = true na criação

            // --- CreateVolumeRequest para Volume (Mutação de Criação) ---
            CreateMap<CreateVolumeRequest, Artigo.Intf.Entities.Volume>()
                // Ignora campos de controle que serão gerados pelo Repositório/Serviço
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ArtigoIds, opt => opt.Ignore())
                .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.ImagemCapa, opt => opt.Ignore()) // ImagemCapa será adicionada depois
                .ForMember(dest => dest.Status, opt => opt.Ignore()); // O serviço define isso

            // =========================================================================
            // MAPEAMENTOS (DTOs de FORMATO e INPUT)
            // =========================================================================

            // --- DTOs de "Formato" (Saída) ---

            // Entidade Artigo -> ArtigoCardListDTO (Formato Card List)
            CreateMap<Artigo.Intf.Entities.Artigo, ArtigoCardListDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo))
                .ForMember(dest => dest.PermitirComentario, opt => opt.MapFrom(src => src.PermitirComentario))
                .ForMember(dest => dest.MidiaDestaque, opt => opt.MapFrom(src => src.MidiaDestaque));

            // Entidade Volume -> VolumeCardDTO (Formato Volume Card)
            CreateMap<Artigo.Intf.Entities.Volume, VolumeCardDTO>();

            // Entidade Autor -> AutorCardDTO (Formato Autor Card)
            CreateMap<Artigo.Intf.Entities.Autor, AutorCardDTO>()
                .ForMember(dest => dest.ArtigoWorkIds, opt => opt.MapFrom(src => src.ArtigoWorkIds))
                .ForMember(dest => dest.Trabalhos, opt => opt.Ignore()); // Será preenchido pelo serviço

            // Entidade Artigo -> ArtigoViewDTO (Formato Artigo View)
            CreateMap<Artigo.Intf.Entities.Artigo, ArtigoViewDTO>()
                .ForMember(dest => dest.AutorReferencias, opt => opt.MapFrom(src => src.AutorReference))
                .ForMember(dest => dest.PermitirComentario, opt => opt.MapFrom(src => src.PermitirComentario))
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo)) // (CAMPO ADICIONADO)
                .ForMember(dest => dest.Autores, opt => opt.Ignore())
                .ForMember(dest => dest.Volume, opt => opt.Ignore())
                .ForMember(dest => dest.ConteudoAtual, opt => opt.Ignore())
                .ForMember(dest => dest.Interacoes, opt => opt.Ignore());

            // Entidade Artigo -> ArtigoEditorialViewDTO (Formato Artigo Editorial View)
            CreateMap<Artigo.Intf.Entities.Artigo, ArtigoEditorialViewDTO>()
                .ForMember(dest => dest.AutorReferencias, opt => opt.MapFrom(src => src.AutorReference))
                .ForMember(dest => dest.Editorial, opt => opt.Ignore())
                .ForMember(dest => dest.ConteudoAtual, opt => opt.Ignore())
                .ForMember(dest => dest.Volume, opt => opt.Ignore())
                .ForMember(dest => dest.Interacoes, opt => opt.Ignore());

            // Entidade Staff -> StaffViewDTO (Formato Staff View)
            CreateMap<Artigo.Intf.Entities.Staff, StaffViewDTO>()
                .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.Job, opt => opt.MapFrom(src => src.Job))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            // Entidade Volume -> VolumeViewDTO (Formato Volume View)
            CreateMap<Artigo.Intf.Entities.Volume, VolumeViewDTO>();


            // --- DTOs de "Input" (Entrada) ---

            // MidiaEntryInputDTO (Entrada) -> Entidade MidiaEntry
            CreateMap<MidiaEntryInputDTO, MidiaEntry>();

            // AutorInputDTO (Entrada) -> Entidade Autor
            CreateMap<AutorInputDTO, Artigo.Intf.Entities.Autor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ArtigoWorkIds, opt => opt.Ignore())
                .ForMember(dest => dest.Contribuicoes, opt => opt.Ignore());

            // CreateStaffRequest (Entrada) -> Entidade Staff
            CreateMap<CreateStaffRequest, Artigo.Intf.Entities.Staff>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
        }
    }
}