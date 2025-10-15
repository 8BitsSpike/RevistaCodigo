using Artigo.API.GraphQL.DataLoaders;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs;
using Artigo.API.GraphQL.Resolvers;
using HotChocolate.Types;
using System.Collections.Generic;
using Artigo.Intf.Entities;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia o ArtigoDTO para um tipo de objeto GraphQL, definindo as bordas (edges) de relacionamento.
    /// </sumario>
    public class ArtigoType : ObjectType<ArtigoDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoDTO> descriptor)
        {
            descriptor.Description("Representa um artigo da revista, incluindo metadados e status editorial.");

            // Campos primários (Mapeamento direto do DTO)
            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().Description("O ID único do artigo.");
            descriptor.Field(f => f.Titulo).Description("Título principal do artigo.");
            descriptor.Field(f => f.Resumo).Description("Resumo/Abstract do conteúdo.");
            descriptor.Field(f => f.Status).Type<NonNullType<EnumType<ArtigoStatus>>>().Description("Status do ciclo de vida editorial.");
            descriptor.Field(f => f.Tipo).Type<NonNullType<EnumType<ArtigoTipo>>>().Description("Classificação do tipo de artigo.");

            // Campos Denormalizados/Métricas
            descriptor.Field(f => f.TotalComentarios).Description("Contagem total de comentários públicos (Denormalizado).");
            descriptor.Field(f => f.TotalInteracoes).Description("Contagem total de interações (Denormalizado).");

            // Relacionamentos Resolvidos (Resolvers definidos em arquivos externos ou no Program.cs)

            // 1. Editorial (1:1)
            descriptor.Field<EditorialResolver>(r => r.GetEditorialAsync(default!, default!, default!))
                .Name("editorial")
                .Type<NonNullType<EditorialType>>()
                .Description("O registro que gerencia o ciclo de vida editorial e revisões do artigo.");

            // 2. Autores (N:M)
            descriptor.Field<AutorResolver>(r => r.GetAutoresAsync(default!, default!, default!, default!))
                .Name("autores")
                .Type<NonNullType<ListType<NonNullType<AutorType>>>>()
                .Description("Os autores e co-autores cadastrados responsáveis pela criação do artigo.");

            // 3. Conteúdo (1:N para ArtigoHistory - Busca a versão atual)
            descriptor.Field<ArtigoHistoryResolver>(r => r.GetCurrentContentAsync(default!, default!, default!))
                .Name("currentContent")
                .Type<StringType>()
                .Description("O conteúdo da versão atual do artigo.");

            // 4. Volume (1:1 Opcional - Busca o Volume apenas se VolumeId existir)
            descriptor.Field<VolumeResolver>(r => r.GetVolumeAsync(default!, default!, default!))
                .Name("volumePublicado")
                .Type<VolumeType>()
                .Description("O Volume (edição da revista) no qual o artigo foi publicado, se aplicável.");
        }
    }
}