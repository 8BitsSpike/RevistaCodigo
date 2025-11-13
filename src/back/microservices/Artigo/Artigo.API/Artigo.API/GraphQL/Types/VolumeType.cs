using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using HotChocolate.Types;
using Artigo.API.GraphQL.Resolvers; // Importado para usar o resolver central
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Artigo.API.GraphQL.DataLoaders;
using System.Linq; // Adicionado para .SelectMany

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia a entidade Volume para um tipo de objeto GraphQL, representando uma edição publicada da revista.
    /// </sumario>
    public class VolumeType : ObjectType<Volume>
    {
        protected override void Configure(IObjectTypeDescriptor<Volume> descriptor)
        {
            descriptor.Description("Representa uma edição publicada da revista (Volume).");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().Description("ID local do Volume.");

            // Metadados da Edição
            descriptor.Field(f => f.Edicao).Description("O número sequencial desta edição.");
            descriptor.Field(f => f.VolumeTitulo).Description("Título temático desta edição.");
            descriptor.Field(f => f.VolumeResumo).Description("Resumo do conteúdo desta edição.");
            descriptor.Field(f => f.M).Type<NonNullType<EnumType<MesVolume>>>().Description("O mês de publicação.");
            descriptor.Field(f => f.N).Description("O número do volume (compatibilidade histórica).");
            descriptor.Field(f => f.Year).Description("O ano de publicação.");
            descriptor.Field(f => f.DataCriacao).Description("Data de criação do registro do volume.");
            descriptor.Field(f => f.ImagemCapa)
                .Type<MidiaEntryType>() // Reutiliza o MidiaEntryType (definido em ArtigoType.cs)
                .Description("Mídia de capa para esta edição.");

            // Relacionamento: Artigos Publicados no Volume (1:N)
            descriptor.Field<ArticleInVolumeResolver>(r => r.GetArticlesAsync(default!, default!, default!))
                .Name("artigos")
                .Type<NonNullType<ListType<NonNullType<ArtigoType>>>>() // Referencia o ArtigoType
                .Description("Todos os artigos publicados que pertencem a esta edição.");
        }
    }
}