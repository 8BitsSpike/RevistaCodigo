using HotChocolate.Types;
using HotChocolate.Data;
using Artigo.API.GraphQL.Queries;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs;
using System.Collections.Generic;

namespace Artigo.API.GraphQL.Types
{
    // ... ArtigoQueryType definition ...
    public class ArtigoQueryType : ObjectType<ArtigoQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoQueries> descriptor)
        {
            // Define o nome do tipo raiz (Obrigatório)
            descriptor.Name("Query");

            // =========================================================================
            // NOVOS CAMPOS TRADUZIDOS E PARA VISITANTES
            // =========================================================================

            // NOVO: 1. Consulta para Visitantes (Público)
            descriptor.Field(f => f.ObterArtigosPublicadosParaVisitantesAsync())
                .Name("obterArtigosPublicadosParaVisitantes")
                .Type<NonNullType<ListType<NonNullType<ArtigoType>>>>()
                .Description("Obtém a lista de todos os artigos com status 'Published'. Não requer autenticação.");

            // RENOMEADO: 2. Consulta de Artigo por ID (Editorial/Staff)
            descriptor.Field(f => f.ObterArtigoPorIdAsync(default!, default!))
                .Name("obterArtigoPorId")
                .Type<ArtigoType>() // Não é NonNull, pois pode retornar nulo se não encontrado ou não autorizado
                .Argument("idArtigo", a => a.Type<NonNullType<IdType>>().Description("O ID único do artigo."))
                .Description("Obtém um artigo por ID (Requer permissão de leitura: Publicado ou Staff/Autor).");

            // RENOMEADO: 3. Consulta de Artigos por Status (Editorial/Staff)
            descriptor.Field(f => f.ObterArtigosPorStatusAsync(default!, default!))
                .Name("obterArtigosPorStatus")
                .Type<NonNullType<ListType<NonNullType<ArtigoType>>>>() // Assumes ArtigoType is registered
                .Argument("status", a => a.Type<NonNullType<EnumType<StatusArtigo>>>().Description("O status editorial para filtrar os artigos.")) // FIX: ArtigoStatus -> StatusArtigo
                .Description("Obtém a lista de artigos filtrados por status (Requer AuthZ/Staff).")

                // HotChocolate Middleware (Ordem correta para MongoDB)
                .UseProjection() // 1. Projection (needs to know what fields to fetch)
                .UseFiltering()  // 2. Filtering
                .UseSorting();   // 3. Sorting (must be last in the data access chain)
        }
    }
}