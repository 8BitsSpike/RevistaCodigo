using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs;
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
            descriptor.Field(f => f.Role).Type<NonNullType<EnumType<FuncaoContribuicao>>>().Description("O papel desempenhado (e.g., AutorPrincipal, Corretor).");
        }
    }

    /// <sumario>
    /// Tipo de objeto GraphQL para as informações de perfil buscadas do UsuarioAPI.
    /// ***  Este tipo é agora obsoleto, mas pode ser mantido se outras partes do schema o usarem  ***
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
    /// Mapeia a entidade Autor para um tipo de objeto GraphQL.
    /// </sumario>
    public class AutorType : ObjectType<Autor>
    {
        protected override void Configure(IObjectTypeDescriptor<Autor> descriptor)
        {
            descriptor.Description("Representa o registro local de um autor, ligando o UsuarioId externo ao histórico de contribuições.");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().Description("ID local do registro do autor.");
            descriptor.Field(f => f.UsuarioId).Type<NonNullType<IdType>>().Description("ID do usuário no sistema externo (UsuarioApi).");

            // CAMPOS (DENORMALIZADOS)
            descriptor.Field(f => f.Nome)
                .Type<NonNullType<StringType>>()
                .Description("Nome de exibição do autor (denormalizado).");

            descriptor.Field(f => f.Url)
                .Type<StringType>()
                .Description("URL da foto de perfil do autor (denormalizado).");

            // Histórico de Contribuições Editoriais (Funções em outros artigos)
            descriptor.Field(f => f.Contribuicoes)
                .Type<NonNullType<ListType<NonNullType<ContribuicaoEditorialType>>>>()
                .Description("Lista de todas as funções editoriais e de autoria que o usuário desempenhou.");
        }
    }
}