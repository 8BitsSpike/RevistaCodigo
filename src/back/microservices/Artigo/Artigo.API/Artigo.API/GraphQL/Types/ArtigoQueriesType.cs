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
            // ... (Other fields are fine)

            // 2. GetArtigosByStatusAsync
            descriptor.Field(f => f.GetArtigosByStatusAsync(default!, default!))
                .Name("artigosByStatus")
                .Type<NonNullType<ListType<NonNullType<ArtigoType>>>>() // Assumes ArtigoType is registered
                .Argument("status", a => a.Type<NonNullType<EnumType<ArtigoStatus>>>())
                .Description("Obtém a lista de artigos filtrados por status (Requer AuthZ/Staff).")

                // --- FIX APPLIED HERE: Reorder Middleware ---
                // The implicit UsePaging often comes first, followed by:
                .UseProjection() // 1. Projection (needs to know what fields to fetch)
                .UseFiltering()  // 2. Filtering
                .UseSorting();   // 3. Sorting (must be last in the data access chain)
                                 // --- END FIX ---
        }
    }
}
