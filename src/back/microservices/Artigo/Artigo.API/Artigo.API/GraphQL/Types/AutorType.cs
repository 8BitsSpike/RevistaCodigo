using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs;
using Artigo.API.GraphQL.DataLoaders; // Necessary for ExternalUserDataLoader
using HotChocolate.Types;
using HotChocolate.Resolvers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

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
            // CORRIGIDO: ContribuicaoRole -> FuncaoContribuicao
            descriptor.Field(f => f.Role).Type<NonNullType<EnumType<FuncaoContribuicao>>>().Description("O papel desempenhado (e.g., AutorPrincipal, Corretor)."); // FIX: ContribuicaoRole -> FuncaoContribuicao
        }
    }

    /// <sumario>
    /// Tipo de objeto GraphQL para as informações de perfil buscadas do UsuarioAPI.
    /// </sumario>
    public class ExternalUserType : ObjectType<ExternalUserDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<ExternalUserDTO> descriptor)
        {
            descriptor.Description("Informações de perfil (nome, media) do Autor, buscadas no UsuarioAPI.");
            descriptor.Field(f => f.Name).Description("Nome de exibição do usuário.");
            descriptor.Field(f => f.MediaUrl).Description("URL da imagem de perfil/avatar.");
        }
    }

    /// <sumario>
    /// Resolver que busca dados externos do UsuarioAPI.
    /// NOTA: Compartilhado entre AutorType e StaffType.
    /// </sumario>
    public class ExternalUserResolver
    {
        public Task<ExternalUserDTO> GetExternalUserAsync(
            [Parent] Autor autor,
            ExternalUserDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            // A chave de busca é o UsuarioId externo. O ! asserts non-null return based on schema.
            return dataLoader.LoadAsync(autor.UsuarioId, cancellationToken)!;
        }
    }

    /// <sumario>
    /// Mapeia a entidade Autor para um tipo de objeto GraphQL.
    /// </sumario>
    public class AutorType : ObjectType<Autor>
    {
        protected override void Configure(IObjectTypeDescriptor<Autor> descriptor)
        {
            descriptor.Description("Representa o registro local de um autor, ligando o UsuarioId externo ao histórico de contribuições.");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().Description("ID local do registro do autor.");
            descriptor.Field(f => f.UsuarioId).Type<NonNullType<IdType>>().Description("ID do usuário no sistema externo (UsuarioApi).");

            // Histórico de Contribuições Editoriais (Funções em outros artigos)
            descriptor.Field(f => f.Contribuicoes)
                .Type<NonNullType<ListType<NonNullType<ContribuicaoEditorialType>>>>()
                .Description("Lista de todas as funções editoriais e de autoria que o usuário desempenhou.");

            // Campo para resolver o nome e media (avatar) do Autor a partir do UsuarioAPI
            descriptor.Field<ExternalUserResolver>(r => r.GetExternalUserAsync(default!, default!, default!))
                .Name("usuarioInfo")
                .Type<ExternalUserType>()
                .Description("Informações de perfil (nome, media) do Autor, buscadas no UsuarioAPI.");
        }
    }
}