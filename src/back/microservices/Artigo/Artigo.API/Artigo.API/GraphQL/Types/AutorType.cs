using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Tipo embutido: Representa o papel de um Autor em uma contribuicao editorial especifica.
    /// </sumario>
    public class ContribuicaoEditorialType : ObjectType<ContribuicaoEditorial>
    {
        protected override void Configure(IObjectTypeDescriptor<ContribuicaoEditorial> descriptor)
        {
            descriptor.Description("Detalha o papel (Role) do autor em um artigo específico (e.g., Revisor, CoAutor).");

            descriptor.Field(f => f.ArtigoId).Type<NonNullType<IdType>>().Description("ID do artigo ao qual esta contribuição se refere.");
            descriptor.Field(f => f.Role).Type<NonNullType<EnumType<ContribuicaoRole>>>().Description("O papel desempenhado (e.g., AutorPrincipal, Corretor).");

            // O Artigo relacionado a esta contribuição pode ser resolvido aqui se necessário.
            // Exemplo:
            // descriptor.Field<ArtigoResolver>(r => r.GetArtigoByIdAsync(default!, default!, default!)).Name("artigo");
        }
    }

    /// <sumario>
    /// Mapeia a entidade Autor para um tipo de objeto GraphQL.
    /// Esta entidade liga o UsuarioId externo ao histórico de trabalho local.
    /// </sumario>
    public class AutorType : ObjectType<Autor>
    {
        protected override void Configure(IObjectTypeDescriptor<Autor> descriptor)
        {
            descriptor.Description("Representa o registro local de um autor, ligando o UsuarioId externo ao histórico de contribuições.");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().Description("ID local do registro do autor.");
            descriptor.Field(f => f.UsuarioId).Type<NonNullType<IdType>>().Description("ID do usuário no sistema externo (UsuarioApi).");

            // Histórico de Artigos (Work)
            descriptor.Field(f => f.ArtigoWorkIds).Description("Lista de IDs de artigos criados ou co-criados.");

            // Histórico de Contribuições Editoriais (Funções em outros artigos)
            descriptor.Field(f => f.Contribuicoes)
                .Type<NonNullType<ListType<NonNullType<ContribuicaoEditorialType>>>>()
                .Description("Lista de todas as funções editoriais e de autoria que o usuário desempenhou.");

            // **TODO:** Campo para resolver o nome e media (avatar) do Autor a partir do UsuarioAPI
            // Exemplo de como um campo externo seria resolvido:
            // descriptor.Field<ExternalUserResolver>(r => r.GetExternalUserAsync(default!, default!))
            //     .Name("usuarioInfo")
            //     .Type<ExternalUserInfoType>() // Tipo que representa Nome, Media, etc.
            //     .Description("Informações de perfil (nome, media) do Autor, buscadas no UsuarioAPI.");
        }
    }

    // NOTA: Resolvers e DataLoaders para Artigos/Contribuicoes serao definidos aqui
    // se precisarmos de carregamento lazy, mas nao sao estritamente necessarios 
    // para a definicao inicial do tipo Autor.
}