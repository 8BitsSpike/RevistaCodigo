using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces; // Added for IReadOnlyList reference if needed
using Artigo.Server.DTOs;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using System.Collections.Generic; // Added
using System.Threading; // Added
using System.Threading.Tasks; // Added
using Artigo.API.GraphQL.DataLoaders; // Added to resolve ArtigoGroupedDataLoader

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
            descriptor.Field(f => f.M).Type<NonNullType<EnumType<VolumeMes>>>().Description("O mês de publicação.");
            descriptor.Field(f => f.N).Description("O número do volume (compatibilidade histórica).");
            descriptor.Field(f => f.Year).Description("O ano de publicação.");
            descriptor.Field(f => f.DataCriacao).Description("Data de criação do registro do volume.");

            // Relacionamento: Artigos Publicados no Volume (1:N)
            descriptor.Field<ArticleInVolumeResolver>(r => r.GetArticlesAsync(default!, default!, default!))
                .Name("artigos")
                .Type<NonNullType<ListType<NonNullType<ArtigoType>>>>() // Referencia o ArtigoType
                .Description("Todos os artigos publicados que pertencem a esta edição.");
        }
    }

    // =========================================================================
    // Resolver para Artigos dentro do Volume
    // =========================================================================

    // Resolver para buscar a lista de Artigos por Volume
    // Resolver para buscar a lista de Artigos por Volume
    public class ArticleInVolumeResolver
    {
        public async Task<IReadOnlyList<ArtigoDTO>> GetArticlesAsync( // FIX: Made method async
            [Parent] Volume volume,
            [Service] ArtigoGroupedDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            // O DataLoader retorna um ILookup. O Hot Chocolate normalmente resolve isso, 
            // mas para forçar o compilador a aceitar a tipagem, usaremos o await e a conversão implícita.

            // NOTE: GroupedDataLoader.LoadAsync returns Task<ILookup<TKey, TValue>>.
            var lookup = await dataLoader.LoadAsync(volume.ArtigoIds, cancellationToken);

            // The compiler expects the final type IReadOnlyList<ArtigoDTO>.
            // We need to flatten the ILookup's result, as the type system is struggling to infer it.
            // Since this resolver is called for a single Volume, we can get the list of values directly.

            // This explicit conversion to IReadOnlyList<T> is required to guarantee the type system's satisfaction.
            return lookup.SelectMany(g => g!).ToList()!; // FIX: Explicitly flatten the ILookup into a List/IReadOnlyList
        }
    }
}