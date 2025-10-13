using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs;

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
    public class ArticleInVolumeResolver
    {
        public Task<IReadOnlyList<ArtigoDTO>> GetArticlesAsync(
            [Parent] Volume volume,
            ArtigoGroupedDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            // O DataLoader usará a lista de ArtigoIds do Volume para buscar os ArtigoDTOs.
            return dataLoader.LoadAsync(volume.ArtigoIds, cancellationToken);
        }
    }
}