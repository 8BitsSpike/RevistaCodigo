﻿using Artigo.API.GraphQL.DataLoaders;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs;
using Artigo.API.GraphQL.Resolvers;
using HotChocolate.Types;
using System.Collections.Generic;
using Artigo.Intf.Entities;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia o DTO da entrada de Mídia para um tipo de objeto GraphQL.
    /// </sumario>
    public class MidiaEntryType : ObjectType<MidiaEntryDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<MidiaEntryDTO> descriptor)
        {
            descriptor.Description("Informações de uma mídia (imagem, vídeo) associada ao artigo.");

            // Campos traduzidos do DTO
            descriptor.Field(f => f.IdMidia).Type<NonNullType<IdType>>().Description("ID de referência da mídia.");
            descriptor.Field(f => f.Url).Type<NonNullType<StringType>>().Description("URL de acesso à mídia.");
            descriptor.Field(f => f.TextoAlternativo).Type<NonNullType<StringType>>().Description("Texto alternativo para SEO e acessibilidade.");
        }
    }

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
            descriptor.Field(f => f.Status).Type<NonNullType<EnumType<StatusArtigo>>>().Description("Status do ciclo de vida editorial.");
            descriptor.Field(f => f.Tipo).Type<NonNullType<EnumType<TipoArtigo>>>().Description("Classificação do tipo de artigo.");

            // Campos Denormalizados/Métricas
            descriptor.Field(f => f.TotalComentarios).Description("Contagem total de comentários públicos (Denormalizado).");
            descriptor.Field(f => f.TotalInteracoes).Description("Contagem total de interações (Denormalizado).");

            // NOVO CAMPO: Mídias Associadas (direto do DTO)
            descriptor.Field(f => f.Midias)
                .Type<NonNullType<ListType<NonNullType<MidiaEntryType>>>>()
                .Description("Lista das mídias (imagem de destaque, vídeos, etc.) associadas ao artigo.");

            // Relacionamentos Resolvidos (Resolvers)

            // 1. Editorial (1:1) - Usa o novo IdEditorial do DTO
            descriptor.Field<EditorialResolver>(r => r.GetEditorialAsync(default!, default!, default!))
                .Name("editorial")
                // REMOVIDO: Argumento redundante que causava CS7036
                .Type<NonNullType<EditorialType>>()
                .Description("O registro que gerencia o ciclo de vida editorial e revisões do artigo.");

            // 2. Autores (N:M) - Usa o novo IdsAutor do DTO
            descriptor.Field<AutorResolver>(r => r.GetAutoresAsync(default!, default!, default!, default!))
                .Name("autores")
                // REMOVIDO: Argumento redundante que causava CS7036
                .Type<NonNullType<ListType<NonNullType<AutorType>>>>()
                .Description("Os autores e co-autores cadastrados responsáveis pela criação do artigo.");

            // 3. Conteúdo (1:N para ArtigoHistory - Busca a versão atual)
            descriptor.Field<ArtigoHistoryResolver>(r => r.GetCurrentContentAsync(default!, default!, default!))
                .Name("currentContent")
                .Type<StringType>()
                .Description("O conteúdo da versão atual do artigo.");

            // 4. Volume (1:1 Opcional - Usa o novo IdVolume do DTO)
            descriptor.Field<VolumeResolver>(r => r.GetVolumeAsync(default!, default!, default!))
                .Name("volumePublicado")
                // REMOVIDO: Argumento redundante que causava CS7036
                .Type<VolumeType>()
                .Description("O Volume (edição da revista) no qual o artigo foi publicado, se aplicável.");
        }
    }
}