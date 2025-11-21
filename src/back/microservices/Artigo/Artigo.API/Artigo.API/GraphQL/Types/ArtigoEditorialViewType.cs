using Artigo.API.GraphQL.DataLoaders;
using Artigo.API.GraphQL.Resolvers; // *** ADICIONADO PARA O RESOLVER CENTRAL ***
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// DTO aninhado para os dados do Editorial (para ArtigoEditorialView).
    /// </sumario>
    public class EditorialViewType : ObjectType<EditorialViewDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<EditorialViewDTO> descriptor)
        {
            descriptor.Description("Dados do ciclo editorial: Posição, ID do histórico atual e Equipe.");
            descriptor.Field(f => f.Position).Type<NonNullType<EnumType<PosicaoEditorial>>>();
            descriptor.Field(f => f.CurrentHistoryId).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.Team).Type<NonNullType<EditorialTeamType>>();
            // Reutiliza o tipo de EditorialType.cs
        }
    }

    /// <sumario>
    /// DTO aninhado para o conteúdo do ArtigoHistory (para ArtigoEditorialView).
    /// </sumario>
    public class ArtigoHistoryEditorialViewType : ObjectType<ArtigoHistoryEditorialViewDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoHistoryEditorialViewDTO> descriptor)
        {
            descriptor.Description("O conteúdo formatado, mídias e comentários internos de uma versão do artigo.");
            descriptor.Field(f => f.Content).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Midias).Type<NonNullType<ListType<NonNullType<MidiaEntryType>>>>();
            descriptor.Field(f => f.StaffComentarios)
                .Type<NonNullType<ListType<NonNullType<StaffComentarioType>>>>() 
                .Description("Comentários internos da equipe editorial sobre esta versão.");
            descriptor.Field(f => f.Version)
                .Type<NonNullType<EnumType<VersaoArtigo>>>()
                .Description("A versão do artigo (e.g., Original, PrimeiraEdicao, Final).");
        }
    }

    /// <sumario>
    /// Mapeia o ArtigoEditorialViewDTO para um tipo de objeto GraphQL.
    /// Representa o 'Artigo Editorial Format'.
    /// </sumario>
    public class ArtigoEditorialViewType : ObjectType<ArtigoEditorialViewDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoEditorialViewDTO> descriptor)
        {
            descriptor.Description("Representa a visualização editorial completa de um artigo, agregando todos os dados de revisão.");

            // =========================================================================
            // Campos Mapeados Diretamente do DTO
            // =========================================================================
            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.Titulo).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Resumo).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Tipo).Type<NonNullType<EnumType<TipoArtigo>>>();
            descriptor.Field(f => f.Status).Type<NonNullType<EnumType<StatusArtigo>>>();
            descriptor.Field(f => f.AutorReferencias)
                .Name("autorReferencias")
                .Type<NonNullType<ListType<NonNullType<StringType>>>>()
                .Description("Nomes de autores não-cadastrados.");

            descriptor.Field(f => f.PermitirComentario)
                .Name("permitirComentario")
                .Type<NonNullType<BooleanType>>();

            // IDs usados pelos resolvers (não expostos)
            descriptor.Field(f => f.AutorIds).Ignore();
            descriptor.Field(f => f.VolumeId).Ignore();
            descriptor.Field(f => f.EditorialId).Type<NonNullType<IdType>>();

            // =========================================================================
            // Campos Resolvidos (Usando DataLoaders para N+1)
            // =========================================================================

            // 1. Resolver para Editorial
            descriptor.Field(f => f.Editorial)
                .Name("editorial")
                .Type<EditorialViewType>() // Pode ser nulo
                .Resolve(async ctx =>
                {
                    var dto = ctx.Parent<ArtigoEditorialViewDTO>();
                    var dataLoader = ctx.DataLoader<EditorialDataLoader>();
                    var editorial = await dataLoader.LoadAsync(dto.EditorialId);

                    if (editorial == null) return null;

                    // Mapeia Entidade -> DTO Aninhado
                    return new EditorialViewDTO
                    {
                        Position = editorial.Position,
                        CurrentHistoryId = editorial.CurrentHistoryId,
                        Team = editorial.Team
                    };
                });

            // 2. Resolver para ConteudoAtual (depende do Editorial)
            descriptor.Field(f => f.ConteudoAtual)
                .Name("conteudoAtual")
                .Type<ArtigoHistoryEditorialViewType>() // Pode ser nulo
                .Description("O conteúdo, mídias e comentários internos da versão atual do artigo.")
                .Resolve(async ctx =>
                {
                    var mapper = ctx.Service<IMapper>();
                    var dto = ctx.Parent<ArtigoEditorialViewDTO>();
                    var editorialLoader = ctx.DataLoader<EditorialDataLoader>();
                    var editorial = await editorialLoader.LoadAsync(dto.EditorialId);

                    if (editorial == null) return null;

                    var historyDataLoader = ctx.DataLoader<ArtigoHistoryGroupedDataLoader>();
                    var historyLookup = await historyDataLoader.LoadAsync(editorial.CurrentHistoryId);
                    var history = historyLookup?.FirstOrDefault();

                    if (history == null) return null;

                    // Mapeia Entidade -> DTO Aninhado
                    return new ArtigoHistoryEditorialViewDTO
                    {
                        Content = history.Content,
                        Midias = mapper.Map<List<MidiaEntryDTO>>(history.Midias),
                        Version = history.Version,
                        StaffComentarios = history.StaffComentarios
                    };
                });

            // 3. Resolver para Volume
            descriptor.Field(f => f.Volume)
                .Name("volume")
                .Type<VolumeCardType>() // Pode ser nulo
                .Description("O volume (edição) onde o artigo foi publicado (se aplicável).")
                .Resolve(async ctx =>
                {
                    var dto = ctx.Parent<ArtigoEditorialViewDTO>();
                    if (string.IsNullOrEmpty(dto.VolumeId))
                    {
                        return null;
                    }
                    var dataLoader = ctx.DataLoader<VolumeDataLoader>();
                    var volume = await dataLoader.LoadAsync(dto.VolumeId);

                    if (volume == null) return null;

                    // Mapeia Entidade -> DTO Aninhado
                    return new VolumeCardDTO
                    {
                        Id = volume.Id,
                        VolumeTitulo = volume.VolumeTitulo,
                        VolumeResumo = volume.VolumeResumo,
                        ImagemCapa = volume.ImagemCapa == null ? null : new MidiaEntryDTO
                        {
                            IdMidia = volume.ImagemCapa.MidiaID,
                            Url = volume.ImagemCapa.Url,
                            TextoAlternativo = volume.ImagemCapa.Alt
                        }
                    };
                });

            // 4. Resolver para Interações (Comentários)
            descriptor.Field(f => f.Interacoes) // Usa o nome da propriedade
                 .Name("interacoes")
                 .Type<InteractionConnectionDTOType>()
                 // Aponta para o método ESPECÍFICO que aceita ArtigoEditorialViewDTO
                 // e removemos os argumentos de paginação que não usamos aqui
                 .ResolveWith<ArtigoInteractionsResolver>(r => r.GetEditorialInteractionsAsync(default!, default!, default!));

        }
    }
}