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

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia o VolumeViewDTO para um tipo de objeto GraphQL.
    /// Representa a "Volume View" (visualização completa de um volume).
    /// </sumario>
    public class VolumeViewType : ObjectType<VolumeViewDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<VolumeViewDTO> descriptor)
        {
            descriptor.Description("Representa a visualização pública completa de uma edição (volume) da revista.");

            // Campos Mapeados Diretamente do DTO
            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.Edicao).Type<NonNullType<IntType>>();
            descriptor.Field(f => f.VolumeTitulo).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.VolumeResumo).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.M).Type<NonNullType<EnumType<MesVolume>>>();
            descriptor.Field(f => f.N).Type<NonNullType<IntType>>();
            descriptor.Field(f => f.Year).Type<NonNullType<IntType>>();
            descriptor.Field(f => f.Status).Type<NonNullType<EnumType<StatusVolume>>>();
            descriptor.Field(f => f.DataCriacao).Type<NonNullType<DateTimeType>>();

            descriptor.Field(f => f.ImagemCapa)
                .Type<MidiaEntryType>() // Reutiliza o MidiaEntryType
                .Description("Mídia de capa para esta edição.");

            // ID usado pelo resolver (ignorado no schema)
            descriptor.Field(f => f.ArtigoIds).Ignore();

            // --- Campo Resolvido (N+1 safe) ---

            descriptor.Field("artigos")
                .Type<NonNullType<ListType<NonNullType<ArtigoCardListType>>>>()
                .Description("Lista de artigos publicados (em formato card) nesta edição.")
                .Resolve(async ctx =>
                {
                    var dto = ctx.Parent<VolumeViewDTO>();
                    var dataLoader = ctx.DataLoader<ArtigoGroupedDataLoader>();

                    // 1. Carrega os artigos em lote
                    var artigosLookup = await dataLoader.LoadAsync(dto.ArtigoIds);

                    var artigos = artigosLookup.SelectMany(group => group!); // Achatamento

                    // 2. Filtra nulos e artigos que não estão publicados + Mapeia para Card
                    return artigos
                        .Where(artigo => artigo != null && artigo.Status == StatusArtigo.Publicado)
                        .Select(artigo => new ArtigoCardListDTO
                        {
                            Id = artigo.Id,
                            Titulo = artigo.Titulo,
                            Resumo = artigo.Resumo,
                            MidiaDestaque = artigo.Midias.FirstOrDefault() == null ? null : new MidiaEntryDTO
                            {
                                IdMidia = artigo.Midias.First().IdMidia,
                                Url = artigo.Midias.First().Url,
                                TextoAlternativo = artigo.Midias.First().TextoAlternativo
                            }
                        })
                        .ToList();
                });
        }
    }
}