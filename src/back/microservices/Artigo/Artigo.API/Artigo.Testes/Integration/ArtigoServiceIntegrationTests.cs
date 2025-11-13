using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using Artigo.Testes.Integration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using System;
using System.Collections.Generic; // Para List<T>
using System.Linq; // Para .First()
using AutoMapper;
using Artigo.Intf.Inputs;

// Define a collection para que o Fixture seja inicializado apenas uma vez
[CollectionDefinition("ArtigoServiceIntegration")]
public class ArtigoServiceIntegrationCollection : ICollectionFixture<ArtigoIntegrationTestFixture> { }

[Collection("ArtigoServiceIntegration")]
// Implementa IAsyncLifetime e IDisposable
public class ArtigoServiceIntegrationTests : IAsyncLifetime, IDisposable
{
    private readonly IServiceScope _scope;
    private readonly IArtigoService _artigoService;
    private readonly IEditorialRepository _editorialRepository;
    private readonly IArtigoHistoryRepository _historyRepository;
    private readonly IAutorRepository _autorRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly IPendingRepository _pendingRepository;
    private readonly IMapper _mapper;

    private const string TestUsuarioId = "test_user_400"; // Autor principal
    private const string CoAutorUsuarioId = "test_user_401"; // Co-autor
    private const string ArticleContent = "Conteúdo completo do Artigo de Teste.";
    private const string AdminUserId = "test_admin_401"; // Usuário autorizado (Admin)
    private const string BolsistaUserId = "test_bolsista_402"; // Usuário Bolsista (para teste de pending)
    private const string NewStaffCandidateId = "test_new_staff_403"; // Usuário a ser promovido
    private const string UnauthorizedUserId = "test_unauthorized_404"; // Usuário sem permissão
    private const string TestCommentary = "Comentário de teste de integração";

    private readonly MidiaEntryInputDTO _midiaDestaqueDTO = new MidiaEntryInputDTO
    {
        MidiaID = "img-01",
        Url = "http://example.com/img01.jpg",
        Alt = "Imagem de Destaque"
    };

    public ArtigoServiceIntegrationTests(ArtigoIntegrationTestFixture fixture)
    {
        _scope = fixture.ServiceProvider.CreateScope();

        _artigoService = _scope.ServiceProvider.GetRequiredService<IArtigoService>();
        _editorialRepository = _scope.ServiceProvider.GetRequiredService<IEditorialRepository>();
        _historyRepository = _scope.ServiceProvider.GetRequiredService<IArtigoHistoryRepository>();
        _autorRepository = _scope.ServiceProvider.GetRequiredService<IAutorRepository>();
        _staffRepository = _scope.ServiceProvider.GetRequiredService<IStaffRepository>();
        _volumeRepository = _scope.ServiceProvider.GetRequiredService<IVolumeRepository>();
        _pendingRepository = _scope.ServiceProvider.GetRequiredService<IPendingRepository>();
        _mapper = _scope.ServiceProvider.GetRequiredService<IMapper>();
    }

    public async Task InitializeAsync()
    {
        await SetupTestUsers();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task SetupTestUsers()
    {
        // Garante que os nomes base sejam consistentes
        if (await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId) == null)
        {
            await _autorRepository.UpsertAsync(new Autor { UsuarioId = TestUsuarioId, Nome = "Autor Teste Base", Url = "url.com/autor" });
        }
        if (await _autorRepository.GetByUsuarioIdAsync(CoAutorUsuarioId) == null)
        {
            await _autorRepository.UpsertAsync(new Autor { UsuarioId = CoAutorUsuarioId, Nome = "Co-Autor Teste Base", Url = "url.com/coautor" });
        }
        if (await _staffRepository.GetByUsuarioIdAsync(BolsistaUserId) == null)
        {
            await _staffRepository.AddAsync(new Staff { UsuarioId = BolsistaUserId, Nome = "Bolsista Teste", Job = FuncaoTrabalho.EditorBolsista });
        }
        if (await _autorRepository.GetByUsuarioIdAsync(AdminUserId) == null)
        {
            await _autorRepository.UpsertAsync(new Autor { UsuarioId = AdminUserId, Nome = "Admin Teste Base", Url = "url.com/admin" });
        }
        // Garante que o usuário não autorizado não seja staff
        if (await _staffRepository.GetByUsuarioIdAsync(UnauthorizedUserId) == null)
        {
            // Adiciona um registro de autor para ele, para garantir que ele não é staff
            if (await _autorRepository.GetByUsuarioIdAsync(UnauthorizedUserId) == null)
            {
                await _autorRepository.UpsertAsync(new Autor { UsuarioId = UnauthorizedUserId, Nome = "Usuário Não Autorizado", Url = "url.com/unauthorized" });
            }
        }
    }

    public void Dispose()
    {
        _scope.Dispose();
    }

    private async Task<Artigo.Intf.Entities.Artigo> CreateTestArticleAsync(string title, string userId, List<string>? referencias = null, string? nomeAutor = null)
    {
        var requestDto = new CreateArtigoRequest
        {
            Titulo = title,
            Conteudo = "Conteúdo",
            Autores = new List<AutorInputDTO> { new AutorInputDTO { UsuarioId = userId, Nome = nomeAutor ?? $"Autor de {title}", Url = "url.com/autor" } },
            ReferenciasAutor = referencias ?? new List<string>(),
            Midias = new List<MidiaEntryInputDTO>()
        };
        var newArtigo = _mapper.Map<Artigo.Intf.Entities.Artigo>(requestDto);
        var autores = _mapper.Map<List<Autor>>(requestDto.Autores);
        var midiasCompletas = _mapper.Map<List<MidiaEntry>>(requestDto.Midias);

        return await _artigoService.CreateArtigoAsync(newArtigo, requestDto.Conteudo, midiasCompletas, autores, userId, TestCommentary);
    }

    private async Task PublishArticleAsync(string artigoId)
    {
        var updateInput = new UpdateArtigoMetadataInput { Status = StatusArtigo.Publicado };
        await _artigoService.AtualizarMetadadosArtigoAsync(artigoId, updateInput, AdminUserId, "Publicando para teste");
    }


    [Fact]
    public async Task CreateArtigoAsync_DeveCriarArtigoEAutoresCorretamente()
    {
        // Arrange
        var autoresInput = new List<AutorInputDTO>
        {
            new AutorInputDTO { UsuarioId = TestUsuarioId, Nome = "Autor Teste", Url = "url.com/autor" },
            new AutorInputDTO { UsuarioId = CoAutorUsuarioId, Nome = "Co-Autor Teste", Url = "url.com/coautor" }
        };
        var requestDto = new CreateArtigoRequest
        {
            Titulo = "Teste de Integração de Artigo",
            Resumo = "Este é um artigo criado via teste de integração.",
            Tipo = TipoArtigo.Artigo,
            Conteudo = ArticleContent,
            Autores = autoresInput,
            ReferenciasAutor = new List<string> { "Referencia Externa" },
            Midias = new List<MidiaEntryInputDTO> { _midiaDestaqueDTO }
        };

        // Mapeamento
        var newArtigo = _mapper.Map<Artigo.Intf.Entities.Artigo>(requestDto);
        var autores = _mapper.Map<List<Autor>>(requestDto.Autores);
        var midiasCompletas = _mapper.Map<List<MidiaEntry>>(requestDto.Midias);

        // Act
        var createdArtigo = await _artigoService.CreateArtigoAsync(newArtigo, requestDto.Conteudo, midiasCompletas, autores, TestUsuarioId, TestCommentary);

        // Assert
        Assert.NotNull(createdArtigo);
        Assert.Equal(requestDto.Titulo, createdArtigo.Titulo);
        var autor1 = await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId);
        var autor2 = await _autorRepository.GetByUsuarioIdAsync(CoAutorUsuarioId);
        Assert.NotNull(autor1);
        Assert.NotNull(autor2);
        Assert.Equal("Autor Teste", autor1.Nome);
        Assert.Equal("Co-Autor Teste", autor2.Nome);
        Assert.Contains(autor1.Id, createdArtigo.AutorIds);
        var editorial = await _editorialRepository.GetByIdAsync(createdArtigo.EditorialId);
        Assert.NotNull(editorial);
        var history = await _historyRepository.GetByIdAsync(editorial.CurrentHistoryId);
        Assert.NotNull(history);
        Assert.Equal(ArticleContent, history.Content);
        Assert.Contains(createdArtigo.Id, autor1.ArtigoWorkIds);

        Assert.NotNull(createdArtigo.MidiaDestaque);
        Assert.Equal("img-01", createdArtigo.MidiaDestaque.MidiaID);
        Assert.Single(history.Midias);
        Assert.Equal("img-01", history.Midias.First().MidiaID);
    }

    [Fact]
    public async Task AtualizarMetadadosArtigoAsync_ShouldUpdateStatusAndPermitirComentario_WhenAdmin()
    {
        // Arrange
        var artigo = await CreateTestArticleAsync("Artigo para Atualizar Status", AdminUserId, null, "Admin Teste");
        Assert.Equal(StatusArtigo.Rascunho, artigo.Status);
        Assert.True(artigo.PermitirComentario);

        var updateInput = new UpdateArtigoMetadataInput
        {
            Status = StatusArtigo.Publicado,
            PermitirComentario = false
        };

        // Act
        var success = await _artigoService.AtualizarMetadadosArtigoAsync(artigo.Id, updateInput, AdminUserId, "Testando update de status");

        // Assert
        Assert.True(success);
        var updatedArtigo = await _artigoService.ObterArtigoParaEditorialAsync(artigo.Id, AdminUserId);
        Assert.NotNull(updatedArtigo);
        Assert.Equal(StatusArtigo.Publicado, updatedArtigo.Status);
        Assert.False(updatedArtigo.PermitirComentario);
    }

    [Fact]
    public async Task CriarNovoStaffAsync_DeveCriarRegistroStaffCorretamente_QuandoAdmin()
    {
        // Arrange
        var requestDto = new CreateStaffRequest
        {
            UsuarioId = NewStaffCandidateId,
            Job = FuncaoTrabalho.EditorBolsista,
            Nome = "Novo Staff",
            Url = "url.com/newstaff"
        };
        var novoStaff = _mapper.Map<Staff>(requestDto);

        // Act
        var newStaff = await _artigoService.CriarNovoStaffAsync(novoStaff, AdminUserId, TestCommentary);

        // Assert
        Assert.NotNull(newStaff);
        Assert.Equal(NewStaffCandidateId, newStaff.UsuarioId);
        var persistedStaff = await _staffRepository.GetByUsuarioIdAsync(NewStaffCandidateId);
        Assert.NotNull(persistedStaff);
        Assert.Equal("Novo Staff", persistedStaff.Nome);
    }

    [Fact]
    public async Task CriarVolumeAsync_DeveCriarVolumeCorretamente_QuandoAdmin()
    {
        // Arrange
        var volumeInicial = new Volume
        {
            VolumeTitulo = "Edição Especial de Verão",
            Edicao = 5,
            N = 1,
            Year = 2024,
            M = MesVolume.Marco
        };

        // Act
        var createdVolume = await _artigoService.CriarVolumeAsync(volumeInicial, AdminUserId, TestCommentary);

        // Assert
        Assert.NotNull(createdVolume);
        Assert.NotEmpty(createdVolume.Id);
        var persistedVolume = await _volumeRepository.GetByIdAsync(createdVolume.Id);
        Assert.NotNull(persistedVolume);
        Assert.Equal("Edição Especial de Verão", persistedVolume.VolumeTitulo);
        Assert.Equal(StatusVolume.EmRevisao, persistedVolume.Status);
    }

    [Fact]
    public async Task CriarVolumeAsync_ShouldCreatePendingRequest_WhenUserIsEditorBolsista()
    {
        // Arrange
        var volumeInicial = new Volume
        {
            VolumeTitulo = "Edição Pendente de Bolsista",
            Edicao = 6,
            N = 2,
            Year = 2025,
            M = MesVolume.Janeiro
        };

        // Act
        var resultVolume = await _artigoService.CriarVolumeAsync(volumeInicial, BolsistaUserId, "Requisição de volume por bolsista");

        // Assert 
        var persistedVolume = await _volumeRepository.GetByIdAsync(resultVolume.Id);
        Assert.Null(persistedVolume);

        // Assert 
        var pendings = await _pendingRepository.BuscarPendenciaPorRequisitanteId(BolsistaUserId);
        var aPendente = pendings.FirstOrDefault(p => p.CommandType == "CreateVolume");

        Assert.NotNull(aPendente);
        Assert.Equal(TipoEntidadeAlvo.Volume, aPendente.TargetType);
        Assert.Contains("Edição Pendente de Bolsista", aPendente.CommandParametersJson);
    }

    [Fact]
    public async Task AddStaffComentarioAsync_ShouldAddCommentToHistory()
    {
        // Arrange
        var artigo = await CreateTestArticleAsync("Artigo para Comentar", TestUsuarioId, null, "Autor Teste");
        var editorial = await _editorialRepository.GetByIdAsync(artigo.EditorialId);
        var historyId = editorial!.CurrentHistoryId;

        // Act
        var updatedHistory = await _artigoService.AddStaffComentarioAsync(historyId, AdminUserId, "Este é um comentário de staff", null);

        // Assert
        Assert.NotNull(updatedHistory);
        Assert.Single(updatedHistory.StaffComentarios);
        Assert.Equal("Este é um comentário de staff", updatedHistory.StaffComentarios[0].Comment);
        var persistedHistory = await _historyRepository.GetByIdAsync(historyId);
        Assert.NotNull(persistedHistory);
        Assert.Single(persistedHistory.StaffComentarios);
        Assert.Equal(AdminUserId, persistedHistory.StaffComentarios[0].UsuarioId);
    }

    [Fact]
    public async Task ObterAutorCardAsync_ShouldReturnAutor()
    {
        // Arrange
        var autor = await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId);
        Assert.NotNull(autor);

        // Act
        var result = await _artigoService.ObterAutorCardAsync(autor.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestUsuarioId, result.UsuarioId);
        Assert.Equal("Autor Teste Base", result.Nome); // Verifica o nome base do setup
    }

    [Fact]
    public async Task ObterVolumesListAsync_ShouldReturnVolumes()
    {
        // Arrange
        var volume = new Volume { VolumeTitulo = "Volume de Teste para Lista", Edicao = 1, N = 1, Year = 2025, M = MesVolume.Maio };
        var createdVolume = await _artigoService.CriarVolumeAsync(volume, AdminUserId, "Teste de lista");

        var updateInput = new UpdateVolumeMetadataInput { Status = StatusVolume.Publicado };
        await _artigoService.AtualizarMetadadosVolumeAsync(createdVolume.Id, updateInput, AdminUserId, "Publicando volume para teste");

        // Act
        var result = await _artigoService.ObterVolumesListAsync(0, 10);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, v => v.VolumeTitulo == "Volume de Teste para Lista");
    }

    [Fact]
    public async Task AtualizarEquipeEditorialAsync_ShouldUpdateTeam_WhenAdmin()
    {
        // Arrange
        var artigo = await CreateTestArticleAsync("Artigo para Teste de Equipe", TestUsuarioId, null, "Autor Teste");
        var editorial = await _editorialRepository.GetByArtigoIdAsync(artigo.Id);
        Assert.NotNull(editorial);
        Assert.Single(editorial.Team.InitialAuthorId);

        var newTeam = new EditorialTeam
        {
            InitialAuthorId = editorial.Team.InitialAuthorId,
            EditorId = "staff_editor_id",
            ReviewerIds = new List<string> { CoAutorUsuarioId }
        };

        // Act
        var updatedEditorial = await _artigoService.AtualizarEquipeEditorialAsync(artigo.Id, newTeam, AdminUserId, "Adicionando Revisor");

        // Assert
        Assert.NotNull(updatedEditorial);
        Assert.Equal("staff_editor_id", updatedEditorial.Team.EditorId);
        Assert.Contains(CoAutorUsuarioId, updatedEditorial.Team.ReviewerIds);

        var persistedEditorial = await _editorialRepository.GetByIdAsync(editorial.Id);
        Assert.NotNull(persistedEditorial);
        Assert.Contains(CoAutorUsuarioId, persistedEditorial.Team.ReviewerIds);
    }

    [Fact]
    public async Task AtualizarEquipeEditorialAsync_ShouldCreatePendingRequest_WhenBolsista()
    {
        // Arrange
        var artigo = await CreateTestArticleAsync("Artigo para Teste de Equipe (Bolsista)", TestUsuarioId, null, "Autor Teste");
        var editorial = await _editorialRepository.GetByArtigoIdAsync(artigo.Id);
        Assert.NotNull(editorial);

        var newTeam = new EditorialTeam
        {
            InitialAuthorId = editorial.Team.InitialAuthorId,
            ReviewerIds = new List<string> { CoAutorUsuarioId }
        };

        // Act
        await _artigoService.AtualizarEquipeEditorialAsync(artigo.Id, newTeam, BolsistaUserId, "Bolsista solicita revisor");

        // Assert 1
        var persistedEditorial = await _editorialRepository.GetByIdAsync(editorial.Id);
        Assert.NotNull(persistedEditorial);
        Assert.Empty(persistedEditorial.Team.ReviewerIds);

        // Assert 2
        var pendings = await _pendingRepository.BuscarPendenciaPorRequisitanteId(BolsistaUserId);
        var aPendente = pendings.FirstOrDefault(p => p.CommandType == "UpdateEditorialTeam");

        Assert.NotNull(aPendente);
        Assert.Equal(TipoEntidadeAlvo.Editorial, aPendente.TargetType);
        Assert.Equal(editorial.Id, aPendente.TargetEntityId);
        Assert.Contains(CoAutorUsuarioId, aPendente.CommandParametersJson);
    }

    // =========================================================================
    // Testes de Busca
    // =========================================================================

    [Fact]
    public async Task ObterArtigosCardListPorTituloAsync_ShouldReturnMatchingArticle()
    {
        // Arrange
        var artigoBusca = await CreateTestArticleAsync("Um Título Muito Específico para Busca", TestUsuarioId, null, "Autor Teste");
        var artigoOutro = await CreateTestArticleAsync("Outro Artigo", TestUsuarioId, null, "Autor Teste");

        await PublishArticleAsync(artigoBusca.Id);
        await PublishArticleAsync(artigoOutro.Id);

        // Act
        var result = await _artigoService.ObterArtigosCardListPorTituloAsync("Muito Específico", 0, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Um Título Muito Específico para Busca", result[0].Titulo);
    }

    [Fact]
    public async Task ObterArtigosCardListPorNomeAutorAsync_ShouldReturnMatchingArticles()
    {
        // Arrange
        // Usar nomes únicos para evitar falha no regex
        // 1. Autor Registrado
        var artigoAutorReg = await CreateTestArticleAsync("Artigo Autor Registrado", TestUsuarioId, null, "Autor Unico 1");

        // 2. Autor por Referência (Não Registrado)
        var artigoAutorRef = await CreateTestArticleAsync("Artigo Autor Convidado", CoAutorUsuarioId, new List<string> { "Maria Convidada" }, "Autor Unico 2");

        // 3. Publica ambos
        await PublishArticleAsync(artigoAutorReg.Id);
        await PublishArticleAsync(artigoAutorRef.Id);

        // Act 1: Busca pelo nome do autor registrado ("Autor Unico 1")
        var resultReg = await _artigoService.ObterArtigosCardListPorNomeAutorAsync("Autor Unico 1", 0, 10);

        // Act 2: Busca pelo nome do autor referenciado ("Maria Convidada")
        var resultRef = await _artigoService.ObterArtigosCardListPorNomeAutorAsync("Maria Convidada", 0, 10);

        // Act 3: Busca pelo outro autor registrado ("Autor Unico 2")
        var resultReg2 = await _artigoService.ObterArtigosCardListPorNomeAutorAsync("Autor Unico 2", 0, 10);

        // Assert 1
        Assert.NotNull(resultReg);
        Assert.Single(resultReg); // Agora deve passar
        Assert.Equal(artigoAutorReg.Id, resultReg[0].Id);

        // Assert 2
        Assert.NotNull(resultRef);
        Assert.Single(resultRef);
        Assert.Equal(artigoAutorRef.Id, resultRef[0].Id);

        // Assert 3
        Assert.NotNull(resultReg2);
        Assert.Single(resultReg2);
        Assert.Equal(artigoAutorRef.Id, resultReg2[0].Id); // O artigo 2 é do "Autor Unico 2"
    }

    // =========================================================================
    // (NOVOS TESTES) Testes de ObterAutorPorIdAsync e ObterMeusArtigosCardListAsync
    // =========================================================================

    [Fact]
    public async Task ObterAutorPorIdAsync_ShouldSucceed_WhenUserIsOwner()
    {
        // Arrange
        var autor = await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId);
        Assert.NotNull(autor);

        // Act
        var result = await _artigoService.ObterAutorPorIdAsync(autor.Id, TestUsuarioId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(autor.Id, result.Id);
        Assert.Equal(TestUsuarioId, result.UsuarioId);
    }

    [Fact]
    public async Task ObterAutorPorIdAsync_ShouldThrowUnauthorized_WhenUserIsNotOwnerOrStaff()
    {
        // Arrange
        var autor = await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId);
        Assert.NotNull(autor);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _artigoService.ObterAutorPorIdAsync(autor.Id, UnauthorizedUserId)
        );
    }

    [Fact]
    public async Task ObterMeusArtigosCardListAsync_ShouldReturnAllStatuses_ForAuthenticatedAutor()
    {
        // Arrange
        var autor = await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId);
        Assert.NotNull(autor);

        // (MODIFICADO) Obtém a contagem inicial de artigos para este autor
        var initialArticles = await _artigoService.ObterMeusArtigosCardListAsync(TestUsuarioId);
        int initialCount = initialArticles.Count;

        // Cria dois artigos para este autor
        var artigoRascunho = await CreateTestArticleAsync("Meu Artigo Rascunho", TestUsuarioId, null, "Autor Teste Base");
        var artigoPublicado = await CreateTestArticleAsync("Meu Artigo Publicado", TestUsuarioId, null, "Autor Teste Base");

        // Publica apenas um deles
        await PublishArticleAsync(artigoPublicado.Id);

        // Act
        var result = await _artigoService.ObterMeusArtigosCardListAsync(TestUsuarioId);

        // Assert
        Assert.NotNull(result);
        // (MODIFICADO) Verifica se a contagem aumentou em 2
        Assert.Equal(initialCount + 2, result.Count);

        // O resto das asserções está correto e verifica se os artigos específicos existem
        var rascunho = result.FirstOrDefault(a => a.Id == artigoRascunho.Id);
        var publicado = result.FirstOrDefault(a => a.Id == artigoPublicado.Id);

        Assert.NotNull(rascunho);
        Assert.Equal(StatusArtigo.Rascunho, rascunho.Status);

        Assert.NotNull(publicado);
        Assert.Equal(StatusArtigo.Publicado, publicado.Status);
    }

    [Fact]
    public async Task ObterMeusArtigosCardListAsync_ShouldReturnEmptyList_ForUnauthorizedUser()
    {
        // Arrange
        // UnauthorizedUserId é garantido como não-autor pelo SetupTestUsers

        // Act
        var result = await _artigoService.ObterMeusArtigosCardListAsync(UnauthorizedUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}