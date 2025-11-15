using HotChocolate.Types;
using HotChocolate.Data;
using Artigo.API.GraphQL.Queries;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs;
using System.Collections.Generic;

namespace Artigo.API.GraphQL.Types
{
    // ... Definições do ArtigoQueryType ...
    public class ArtigoQueryType : ObjectType<ArtigoQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoQueries> descriptor)
        {
            // Define o nome do tipo raiz (Obrigatório)
            descriptor.Name("Query");

            // =========================================================================
            // *** QUERIES PÚBLICAS (Formatos) ***
            // =========================================================================

            // 1. Consulta para 'Card List Format' (Público)
            descriptor.Field(f => f.ObterArtigosCardListAsync(default!, default!))
                .Name("obterArtigosCardList")
                .Type<NonNullType<ListType<NonNullType<ArtigoCardListType>>>>()
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Obtém a lista de artigos publicados em formato de card. Não requer autenticação.");

            // 2. Consulta para 'Card List Format' por Tipo (Público)
            descriptor.Field(f => f.ObterArtigosCardListPorTipoAsync(default!, default!, default!))
                .Name("obterArtigosCardListPorTipo")
                .Type<NonNullType<ListType<NonNullType<ArtigoCardListType>>>>()
                .Argument("tipo", a => a.Type<NonNullType<EnumType<TipoArtigo>>>().Description("O tipo de artigo para filtrar a lista."))
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Obtém a lista de artigos publicados (em formato card) de um tipo específico.");

            // 3. Consulta para 'Card List Format' por Título (Público)
            descriptor.Field(f => f.ObterArtigosCardListPorTituloAsync(default!, default!, default!))
                .Name("obterArtigoCardListPorTitulo")
                .Type<NonNullType<ListType<NonNullType<ArtigoCardListType>>>>()
                .Argument("searchTerm", a => a.Type<NonNullType<StringType>>().Description("O termo a ser buscado no título do artigo."))
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Busca artigos publicados (em formato card) pelo título.");

            // 4. Consulta para 'Card List Format' por Nome do Autor (Público)
            descriptor.Field(f => f.ObterArtigosCardListPorNomeAutorAsync(default!, default!, default!))
                .Name("obterArtigoCardListPorNomeAutor")
                .Type<NonNullType<ListType<NonNullType<ArtigoCardListType>>>>()
                .Argument("searchTerm", a => a.Type<NonNullType<StringType>>().Description("O termo a ser buscado no nome do autor."))
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Busca artigos publicados (em formato card) pelo nome do autor.");

            // 5. Consulta para 'Card List Format' por Lista de IDs (Público)
            descriptor.Field(f => f.ObterArtigosCardListPorListaAsync(default!))
                .Name("obterArtigoCardListPorLista")
                .Type<NonNullType<ListType<NonNullType<ArtigoCardListType>>>>()
                .Argument("ids", a => a.Type<NonNullType<ListType<NonNullType<IdType>>>>().Description("Lista de IDs de artigos a serem buscados."))
                .Description("Obtém múltiplos artigos (em formato card) a partir de uma lista de IDs. Retorna apenas artigos publicados.");

            // 6. Consulta para 'Volume List Format' (Público)
            descriptor.Field(f => f.ObterVolumesListAsync(default!, default!))
                .Name("obterVolumesList")
                .Type<NonNullType<ListType<NonNullType<VolumeCardType>>>>()
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Obtém a lista de volumes (edições) publicados em formato de card.");

            // 7. Consulta para 'Autor Format' (Público)
            descriptor.Field(f => f.ObterAutorViewAsync(default!))
                .Name("obterAutorView")
                .Type<AutorViewType>() // Pode ser nulo se não encontrado
                .Argument("autorId", a => a.Type<NonNullType<IdType>>().Description("O ID local do registro do autor."))
                .Description("Obtém as informações públicas de um autor. Não requer autenticação.");

            // 8. Consulta para 'Artigo Format' (Público)
            descriptor.Field(f => f.ObterArtigoViewAsync(default!))
                .Name("obterArtigoView")
                .Type<ArtigoViewType>() // Pode ser nulo se não encontrado
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>().Description("O ID único do artigo."))
                .Description("Obtém a visualização completa de um artigo publicado. Não requer autenticação.");

            // 9. Consulta para 'Volume View Format' (Público)
            descriptor.Field(f => f.ObterVolumeViewAsync(default!))
                .Name("obterVolumeView")
                .Type<VolumeViewType>() // Pode ser nulo (se não for 'Publicado')
                .Argument("volumeId", a => a.Type<NonNullType<IdType>>().Description("O ID único do volume."))
                .Description("Obtém a visualização pública completa de um volume (edição) publicado.");

            // 10. Consulta para 'Autor Card Format' (Público)
            descriptor.Field(f => f.ObterAutorCardAsync(default!))
                .Name("obterAutorCard")
                .Type<AutorCardType>() // Pode ser nulo
                .Argument("autorId", a => a.Type<NonNullType<IdType>>().Description("O ID local do registro do autor."))
                .Description("Obtém a visualização de card de um autor, incluindo seus trabalhos.");

            // 11. Consulta para 'Volume Card Format' (Público)
            descriptor.Field(f => f.ObterVolumeCardAsync(default!))
                .Name("obterVolumeCard")
                .Type<VolumeCardType>() // Pode ser nulo
                .Argument("volumeId", a => a.Type<NonNullType<IdType>>().Description("O ID único do volume."))
                .Description("Obtém a visualização de card de um volume (edição).");

            // 12. Consulta para Comentários Públicos (Público)
            descriptor.Field(f => f.ObterComentariosPublicosAsync(default!, default!, default!))
                .Name("obterComentariosPublicos")
                .Type<NonNullType<ListType<NonNullType<InteractionType>>>>() // Retorna a entidade Interaction
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>().Description("O ID do artigo para buscar os comentários."))
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Obtém uma lista paginada de comentários públicos de um artigo.");


            // =========================================================================
            // QUERIES INTERNAS (Staff/Autenticado)
            // =========================================================================

            // 13. Verifica se o usuário é Staff
            descriptor.Field(f => f.VerificarStaffAsync(default!))
                .Name("verificarStaff")
                .Type<NonNullType<BooleanType>>()
                .Description("Verifica se o usuário autenticado é um membro ativo da equipe Staff. Requer autenticação.")
                .Authorize();

            // 14. Consulta para 'Artigo Editorial Format' (Staff/Autor)
            descriptor.Field(f => f.ObterArtigoEditorialViewAsync(default!, default!))
                .Name("obterArtigoEditorialView")
                .Type<ArtigoEditorialViewType>() // Pode ser nulo se não encontrado
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>().Description("O ID único do artigo."))
                .Description("Obtém a visualização editorial completa de um artigo. Requer autenticação (Autor ou Staff).");


            // 15. Consulta para Visitantes (Público) - (Mantida)
            descriptor.Field(f => f.ObterArtigosPublicadosParaVisitantesAsync(default!, default!))
                .Name("obterArtigosPublicadosParaVisitantes")
                .Type<NonNullType<ListType<NonNullType<ArtigoType>>>>()
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Obtém a lista de todos os artigos com status 'Publicado'. Não requer autenticação.");

            // 16. Consulta de Artigo por ID (Editorial/Staff)
            descriptor.Field(f => f.ObterArtigoPorIdAsync(default!, default!))
                .Name("obterArtigoPorId")
                .Type<ArtigoType>() // Não é NonNull, pois pode retornar nulo se não encontrado ou não autorizado
                .Argument("idArtigo", a => a.Type<NonNullType<IdType>>().Description("O ID único do artigo."))
                .Description("Obtém um artigo por ID (Requer permissão de leitura: Publicado ou Staff/Autor).")
                .UseProjection();

            // 17. Consulta de Artigos por Status (Editorial/Staff)
            descriptor.Field(f => f.ObterArtigosPorStatusAsync(default!, default!, default!, default!))
                .Name("obterArtigosPorStatus")
                .Type<NonNullType<ListType<NonNullType<ArtigoCardListType>>>>()
                .Argument("status", a => a.Type<NonNullType<EnumType<StatusArtigo>>>().Description("O status editorial para filtrar os artigos."))
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Obtém a lista de artigos (em formato card) filtrados por status (Requer AuthZ/Staff).")
                .Authorize() // (NOVO) Garante que apenas staff pode chamar
                .UseProjection()
                .UseFiltering()
                .UseSorting();

            // 18. Consulta para artigos do próprio autor
            descriptor.Field(f => f.ObterMeusArtigosCardListAsync(default!))
                .Name("obterMeusArtigosCardList")
                .Type<NonNullType<ListType<NonNullType<ArtigoCardListType>>>>()
                .Description("Obtém a lista de artigos (em formato card) do usuário autenticado. Retorna todos os status.")
                .Authorize(); // Garante que o usuário deve estar logado

            // 19. Consulta para Pedidos Pendentes (TODOS) - (ATUALIZADA)
            descriptor.Field(f => f.ObterPendentesAsync(default!, default!, default!, default!, default!, default!, default!))
                .Name("obterPendentes")
                .Type<NonNullType<ListType<NonNullType<PendingType>>>>() // Retorna a lista da entidade Pending
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Argument("status", a => a.Type<EnumType<StatusPendente>>().Description("Filtra por status pendente."))
                .Argument("targetEntityId", a => a.Type<IdType>().Description("Filtra pelo ID da entidade alvo."))
                .Argument("targetType", a => a.Type<EnumType<TipoEntidadeAlvo>>().Description("Filtra pelo tipo da entidade alvo."))
                .Argument("requesterUsuarioId", a => a.Type<IdType>().Description("Filtra pelo ID do usuário requisitante."))
                .Description("Obtém pedidos pendentes (com filtros). Requer AuthZ (EditorChefe/Admin).")
                .Authorize(); // Garante que apenas staff pode chamar

            // --- QUERIES DE AUTOR E VOLUME (PAGINADAS E PONTUAIS) ---

            // 20. Consulta para Autores (Paginada)
            descriptor.Field(f => f.ObterAutoresAsync(default!, default!, default!))
                .Name("obterAutores")
                .Type<NonNullType<ListType<NonNullType<AutorType>>>>()
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Obtém a lista paginada de todos os autores registrados no sistema. Requer permissão de Staff.")
                .Authorize();

            // 21. Consulta para Autor por ID (Pontual)
            descriptor.Field(f => f.ObterAutorPorIdAsync(default!, default!))
                .Name("obterAutorPorId")
                .Type<AutorType>()
                .Argument("idAutor", a => a.Type<NonNullType<IdType>>().Description("O ID local do registro de Autor."))
                .Description("Obtém um registro de Autor específico. Requer permissão de Staff ou ser o próprio autor.")
                .Authorize();

            // 22. Consulta para Volumes (Paginada)
            descriptor.Field(f => f.ObterVolumesAsync(default!, default!, default!))
                .Name("obterVolumes")
                .Type<NonNullType<ListType<NonNullType<VolumeCardType>>>>()
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Obtém a lista paginada de todas as edições (Volumes) da revista em formato card. Requer permissão de Staff.")
                .Authorize();

            // 23. Consulta para Volumes por Ano (Paginada)
            descriptor.Field(f => f.ObterVolumesPorAnoAsync(default!, default!, default!, default!))
                .Name("obterVolumesPorAno")
                .Type<NonNullType<ListType<NonNullType<VolumeType>>>>()
                .Argument("ano", a => a.Type<NonNullType<IntType>>().Description("O ano de publicação para filtrar os volumes."))
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Obtém a lista paginada de edições (Volumes) filtradas por ano. Requer permissão de Staff.")
                .Authorize();

            // 24. Consulta para Volumes por Status (Paginada)
            descriptor.Field(f => f.ObterVolumesPorStatusAsync(default!, default!, default!, default!))
                .Name("obterVolumesPorStatus")
                .Type<NonNullType<ListType<NonNullType<VolumeCardType>>>>()
                .Argument("status", a => a.Type<NonNullType<EnumType<StatusVolume>>>().Description("O status para filtrar os volumes."))
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Obtém a lista paginada de edições (Volumes) em formato card, filtrada por status. Requer permissão de Staff.")
                .Authorize();

            // 25. Consulta para Volume por ID (Pontual)
            descriptor.Field(f => f.ObterVolumePorIdAsync(default!, default!))
                .Name("obterVolumePorId")
                .Type<VolumeType>()
                .Argument("idVolume", a => a.Type<NonNullType<IdType>>().Description("O ID local do Volume."))
                .Description("Obtém um registro de Volume específico. Requer permissão de Staff.")
                .Authorize();

            // 26. Consulta para Staff (Paginada)
            descriptor.Field(f => f.ObterStaffListAsync(default!, default!, default!))
                .Name("obterStaffList")
                .Type<NonNullType<ListType<NonNullType<StaffViewDTOType>>>>() // Mapeia para o DTO
                .Argument("pagina", a => a.Type<NonNullType<IntType>>().Description("O número da página a ser solicitada (começando em 0)."))
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>().Description("O número máximo de itens por página."))
                .Description("Obtém a lista paginada de todos os membros da Staff. Requer permissão de Staff.")
                .Authorize();

            // 27. Consulta para Staff por ID (Pontual)
            descriptor.Field(f => f.ObterStaffPorIdAsync(default!, default!))
                .Name("obterStaffPorId")
                .Type<StaffViewDTOType>() // Mapeia para o DTO
                .Argument("staffId", a => a.Type<NonNullType<IdType>>().Description("O ID local do registro de Staff."))
                .Description("Obtém um registro de Staff específico. Requer permissão de Staff.")
                .Authorize();

            // --- (NOVAS QUERIES DE STAFF) ---

            // 28. (NOVO) Busca editorial por Tipo
            descriptor.Field(f => f.ObterArtigosEditorialPorTipoAsync(default!, default!, default!, default!))
                .Name("obterArtigosEditorialPorTipo")
                .Type<NonNullType<ListType<NonNullType<ArtigoCardListType>>>>()
                .Argument("tipo", a => a.Type<NonNullType<EnumType<TipoArtigo>>>())
                .Argument("pagina", a => a.Type<NonNullType<IntType>>())
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>())
                .Description("Busca artigos (formato card) por tipo, sem filtro de status. Requer Staff.")
                .Authorize();

            // 29. (NOVO) Busca editorial por Título
            descriptor.Field(f => f.SearchArtigosEditorialByTitleAsync(default!, default!, default!, default!))
                .Name("searchArtigosEditorialByTitle")
                .Type<NonNullType<ListType<NonNullType<ArtigoCardListType>>>>()
                .Argument("searchTerm", a => a.Type<NonNullType<StringType>>())
                .Argument("pagina", a => a.Type<NonNullType<IntType>>())
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>())
                .Description("Busca artigos (formato card) por título, sem filtro de status. Requer Staff.")
                .Authorize();

            // 30. (NOVO) Busca editorial por IDs de Autor
            descriptor.Field(f => f.SearchArtigosEditorialByAutorIdsAsync(default!, default!, default!, default!))
                .Name("searchArtigosEditorialByAutorIds")
                .Type<NonNullType<ListType<NonNullType<ArtigoCardListType>>>>()
                .Argument("idsAutor", a => a.Type<NonNullType<ListType<NonNullType<IdType>>>>())
                .Argument("pagina", a => a.Type<NonNullType<IntType>>())
                .Argument("tamanho", a => a.Type<NonNullType<IntType>>())
                .Description("Busca artigos (formato card) por IDs de autor, sem filtro de status. Requer Staff.")
                .Authorize();
        }
    }
}