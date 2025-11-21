using HotChocolate.Types;
using HotChocolate.Data;
using Artigo.API.GraphQL.Queries;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs;
using System.Collections.Generic;

namespace Artigo.API.GraphQL.Types
{
    public class ArtigoQueryType : ObjectType<ArtigoQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoQueries> descriptor)
        {
            descriptor.Name("Query");

            descriptor.Field(f => f.ObterArtigosCardListAsync(default!, default!)).Name("obterArtigosCardList");
            descriptor.Field(f => f.ObterArtigosCardListPorTipoAsync(default!, default!, default!)).Name("obterArtigosCardListPorTipo");
            descriptor.Field(f => f.ObterArtigosCardListPorTituloAsync(default!, default!, default!)).Name("obterArtigoCardListPorTitulo");
            descriptor.Field(f => f.ObterArtigosCardListPorNomeAutorAsync(default!, default!, default!)).Name("obterArtigoCardListPorNomeAutor");
            descriptor.Field(f => f.ObterArtigosCardListPorListaAsync(default!)).Name("obterArtigoCardListPorLista");
            descriptor.Field(f => f.ObterVolumesListAsync(default!, default!)).Name("obterVolumesList");
            descriptor.Field(f => f.ObterAutorViewAsync(default!)).Name("obterAutorView");
            descriptor.Field(f => f.ObterArtigoViewAsync(default!)).Name("obterArtigoView");
            descriptor.Field(f => f.ObterVolumeViewAsync(default!)).Name("obterVolumeView");
            descriptor.Field(f => f.ObterAutorCardAsync(default!)).Name("obterAutorCard");
            descriptor.Field(f => f.ObterVolumeCardAsync(default!)).Name("obterVolumeCard");
            descriptor.Field(f => f.ObterComentariosPublicosAsync(default!, default!, default!)).Name("obterComentariosPublicos");

            // --- Queries Internas  ---

            descriptor.Field(f => f.VerificarStaffAsync())
                .Name("verificarStaff");

            descriptor.Field(f => f.ObterArtigoEditorialViewAsync(default!))
                .Name("obterArtigoEditorialView");

            descriptor.Field(f => f.ObterArtigosPublicadosParaVisitantesAsync(default!, default!))
                .Name("obterArtigosPublicadosParaVisitantes");

            descriptor.Field(f => f.ObterArtigoPorIdAsync(default!))
                .Name("obterArtigoPorId");

            descriptor.Field(f => f.ObterArtigosPorStatusAsync(default!, default!, default!))
                .Name("obterArtigosPorStatus");

            descriptor.Field(f => f.ObterMeusArtigosCardListAsync())
                .Name("obterMeusArtigosCardList");

            descriptor.Field(f => f.ObterPendentesAsync(default!, default!, default!, default!, default!, default!))
                .Name("obterPendentes");

            descriptor.Field(f => f.ObterAutoresAsync(default!, default!))
                .Name("obterAutores");

            descriptor.Field(f => f.ObterAutorPorIdAsync(default!))
                .Name("obterAutorPorId");

            descriptor.Field(f => f.ObterVolumesAsync(default!, default!))
                .Name("obterVolumes");

            descriptor.Field(f => f.ObterVolumesPorAnoAsync(default!, default!, default!))
                .Name("obterVolumesPorAno");

            descriptor.Field(f => f.ObterVolumesPorStatusAsync(default!, default!, default!))
                .Name("obterVolumesPorStatus");

            descriptor.Field(f => f.ObterVolumePorIdAsync(default!))
                .Name("obterVolumePorId");

            descriptor.Field(f => f.ObterStaffPorIdAsync(default!))
                .Name("obterStaffPorId");

            descriptor.Field(f => f.ObterStaffListAsync(default!, default!))
                .Name("obterStaffList");

            descriptor.Field(f => f.ObterArtigosEditorialPorTipoAsync(default!, default!, default!))
                .Name("obterArtigosEditorialPorTipo");

            descriptor.Field(f => f.SearchArtigosEditorialByTitleAsync(default!, default!, default!))
                .Name("searchArtigosEditorialByTitle");

            descriptor.Field(f => f.SearchArtigosEditorialByAutorIdsAsync(default!, default!, default!))
                .Name("searchArtigosEditorialByAutorIds");
        }
    }
}