using Artigo.API.GraphQL.DataLoaders;
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
using Artigo.API.GraphQL.Resolvers; 

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// DTO aninhado para o conteúdo do ArtigoHistory no ArtigoView.
    /// </sumario>
    public class ArtigoHistoryViewType : ObjectType<ArtigoHistoryViewDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoHistoryViewDTO> descriptor)
        {
            descriptor.Description("O conteúdo formatado e as mídias de uma versão específica do artigo.");
            descriptor.Field(f => f.Content).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Midias).Type<NonNullType<ListType<NonNullType<MidiaEntryType>>>>();
        }
    }

    /// <sumario>
    /// DTO aninhado para a conexão de Interações, permitindo paginação.
    /// </sumario>
    public class InteractionConnectionDTOType : ObjectType<InteractionConnectionDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<InteractionConnectionDTO> descriptor)
        {
            descriptor.Description("Uma conexão paginada de comentários, separada por tipo.");
            descriptor.Field(f => f.TotalComentariosPublicos).Type<NonNullType<IntType>>();
            descriptor.Field(f => f.ComentariosEditoriais).Type<NonNullType<ListType<NonNullType<InteractionType>>>>();
            descriptor.Field(f => f.ComentariosPublicos).Type<NonNullType<ListType<NonNullType<InteractionType>>>>();
        }
    }


    /// <sumario>
    /// Mapeia o ArtigoViewDTO para um tipo de objeto GraphQL.
    /// Representa o 'Artigo Format' (visualização completa do artigo).
    /// </sumario>
    public class ArtigoViewType : ObjectType<ArtigoViewDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoViewDTO> descriptor)
        {
            descriptor.Description("Representa a visualização completa de um artigo publicado, agregando dados do volume e autores.");

            // =========================================================================
            // Campos Mapeados Diretamente do DTO
            // =========================================================================
            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.Titulo).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.AutorReferencias)
                .Name("autorReferencias")
                .Type<NonNullType<ListType<NonNullType<StringType>>>>()
                .Description("Nomes de autores não-cadastrados.");

            descriptor.Field(f => f.PermitirComentario)
                .Name("permitirComentario")
                .Type<NonNullType<BooleanType>>();

            descriptor.Field(f => f.Tipo)
                .Type<NonNullType<EnumType<TipoArtigo>>>()
                .Description("O tipo do artigo (e.g., Artigo, Blog).");

            // IDs usados pelos resolvers (não expostos diretamente se não necessário)
            descriptor.Field(f => f.AutorIds).Ignore();
            descriptor.Field(f => f.VolumeId).Ignore();
            descriptor.Field(f => f.EditorialId).Ignore();


            // =========================================================================
            // Campos Resolvidos (Usando DataLoaders para N+1)
            // =========================================================================

            // 1. Resolver para Autores
            descriptor.Field(f => f.Autores)
                .Name("autores")
                .Type<NonNullType<ListType<NonNullType<AutorViewType>>>>()
                .Description("Lista de autores denormalizados (ID, Nome, Url) associados a este artigo.")
                .Resolve(async ctx =>
                {
                    var dto = ctx.Parent<ArtigoViewDTO>();
                    var dataLoader = ctx.DataLoader<AutorBatchDataLoader>();
                    var autores = await dataLoader.LoadAsync(dto.AutorIds);

                    return autores
                        .Where(a => a != null)
                        .Select(a => new AutorViewDTO { UsuarioId = a!.UsuarioId, Nome = a.Nome, Url = a.Url });
                });

            // 2. Resolver para Volume
            descriptor.Field(f => f.Volume)
                .Name("volume")
                .Type<VolumeCardType>() // Pode ser nulo
                .Description("O volume (edição) onde o artigo foi publicado.")
                .Resolve(async ctx =>
                {
                    var dto = ctx.Parent<ArtigoViewDTO>();
                    if (string.IsNullOrEmpty(dto.VolumeId))
                    {
                        return null;
                    }
                    var dataLoader = ctx.DataLoader<VolumeDataLoader>();
                    var volume = await dataLoader.LoadAsync(dto.VolumeId);

                    if (volume == null)
                    {
                        return null;
                    }

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

            // 3. Resolver para ConteudoAtual
            descriptor.Field(f => f.ConteudoAtual)
                .Name("conteudoAtual")
                .Type<ArtigoHistoryViewType>() // Pode ser nulo
                .Description("O conteúdo e mídias da versão atual (publicada) do artigo.")
                .Resolve(async ctx =>
                {
                    var dto = ctx.Parent<ArtigoViewDTO>();
                    var editorialLoader = ctx.DataLoader<EditorialDataLoader>();

                    var editorial = await editorialLoader.LoadAsync(dto.EditorialId);
                    if (editorial == null) return null;

                    var historyDataLoader = ctx.DataLoader<ArtigoHistoryGroupedDataLoader>();
                    var historyLookup = await historyDataLoader.LoadAsync(editorial.CurrentHistoryId);
                    var history = historyLookup!.FirstOrDefault();

                    if (history == null) return null;

                    return new ArtigoHistoryViewDTO
                    {
                        Content = history.Content,
                        Midias = history.Midias.Select(m => new MidiaEntryDTO
                        {
                            IdMidia = m.MidiaID,
                            Url = m.Url,
                            TextoAlternativo = m.Alt
                        }).ToList()
                    };
                });

            // 4. Resolver para Interações (Comentários)
            descriptor.Field<ArtigoInteractionsResolver>(r => r.GetInteractionsAsync(default!, default!, default!, default!, default!))
                .Name("interacoes")
                .Argument("page", a => a.Type<IntType>().DefaultValue(0))
                .Argument("pageSize", a => a.Type<IntType>().DefaultValue(10))
                .Type<InteractionConnectionDTOType>()
                .Description("Comentários editoriais e uma lista paginada de comentários públicos.");
        }
    }
}