using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    // DataLoader para Artigo (GroupedDataLoader, reutiliza o conceito para ArtigoDTO)
    // Este DataLoader precisa ser criado na pasta Artigo.API/GraphQL/DataLoaders/
    public class ArtigoGroupedDataLoader : GroupedDataLoader<string, ArtigoDTO>
    {
        private readonly IArtigoService _artigoService;

        public ArtigoGroupedDataLoader(IBatchScheduler scheduler, IArtigoService artigoService) : base(scheduler)
        {
            _artigoService = artigoService;
        }

        protected override async Task<ILookup<string, ArtigoDTO>> LoadGroupedBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            // Nota: Esta lógica é complexa. O serviço não tem um GetByIdsAsync que retorna ArtigoDTOs diretamente.
            // Para simplificar a implementação, assumimos que ArtigoService precisa expor um método
            // para buscar ArtigoDTOs por IDs, mas, na arquitetura, o serviço normalmente retorna Artigos (Entities).
            // A solução correta é usar o ArtigoRepository para buscar Artigo entities e mapeá-las aqui.

            // Para prosseguir, usaremos o ArtigoRepository e AutoMapper diretamente aqui.
            // Isso requer injetar o IArtigoRepository e o IMapper, o que é comum em DataLoaders.

            throw new NotImplementedException("Este DataLoader requer injetar IArtigoRepository e IMapper. Prossiga com a criação do ArtigoType e a configuração do DI.");

            /* Lógica Correta (exigiria refatoração do DI):
            var artigos = await _artigoRepository.GetByIdsAsync(keys.ToList());
            var dtos = _mapper.Map<IReadOnlyList<ArtigoDTO>>(artigos);
            return dtos.ToLookup(a => a.Id, a => a);
            */
        }
    }
}