using Artigo.API.GraphQL.DataLoaders; 
using Artigo.Server.DTOs;
using HotChocolate; 
using HotChocolate.Types;
using System.Linq; 
using System.Threading.Tasks; 

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia o AutorTrabalhoDTO (um artigo em que o autor trabalhou)
    /// para um tipo de objeto GraphQL.
    /// </sumario>
    public class AutorTrabalhoDTOType : ObjectType<AutorTrabalhoDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<AutorTrabalhoDTO> descriptor)
        {
            descriptor.Description("Um trabalho (artigo) associado a um autor.");
            descriptor.Field(f => f.ArtigoId).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.Titulo).Type<NonNullType<StringType>>();
        }
    }

    /// <sumario>
    /// Mapeia o AutorCardDTO para um tipo de objeto GraphQL.
    /// Representa o 'Autor Card' format.
    /// </sumario>
    public class AutorCardType : ObjectType<AutorCardDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<AutorCardDTO> descriptor)
        {
            descriptor.Description("Representa um autor em formato de 'card' resumido.");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.UsuarioId).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.Nome).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Url).Type<StringType>();

            // Campo de IDs (ignorado, usado apenas pelo resolver)
            descriptor.Field(f => f.ArtigoWorkIds).Ignore();
            descriptor.Field(f => f.Trabalhos)
                .Type<NonNullType<ListType<NonNullType<AutorTrabalhoDTOType>>>>()
                .Description("Lista de títulos de artigos em que este autor trabalhou.")
                .Resolve(async ctx =>
                {
                    var dto = ctx.Parent<AutorCardDTO>();
                    var dataLoader = ctx.DataLoader<ArtigoGroupedDataLoader>();

                    // 1. Carrega os artigos em lote
                    var artigos = await dataLoader.LoadAsync(dto.ArtigoWorkIds);

                    // 2. Filtra nulos e mapeia para o DTO de Trabalho
                    // O ArtigoGroupedDataLoader retorna ILookup<string, ArtigoDTO>
                    // Achata(Flatten) o lookup e filtra os nulos.
                    return artigos
                        .SelectMany(group => group!) // Achatamento
                        .Where(artigo => artigo != null)
                        .Select(artigo => new AutorTrabalhoDTO
                        {
                            ArtigoId = artigo.Id,
                            Titulo = artigo.Titulo
                        })
                        .ToList();
                });
        }
    }
}