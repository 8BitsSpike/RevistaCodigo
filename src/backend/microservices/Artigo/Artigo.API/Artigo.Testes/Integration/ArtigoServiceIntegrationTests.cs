using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using Artigo.Testes.Integration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Artigo.Intf.Inputs;
using System.Text.Json;

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
    private const string InactiveStaffId = "test_inactive_405";
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
        // FIX 1: Ensure names are explicitly set, fixing the ObterAutorCardAsync failure.
        if (await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId) == null)
        {
            await _autorRepository.UpsertAsync(new Autor { UsuarioId = TestUsuarioId, Nome = "Autor Teste Base", Url = "url.com/autor" });
        }
        else
        {
            var autor = await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId);
            autor!.Nome = "Autor Teste Base";
            await _autorRepository.UpsertAsync(autor);
        }

        if (await _autorRepository.GetByUsuarioIdAsync(CoAutorUsuarioId) == null)
        {
            await _autorRepository.UpsertAsync(new Autor { UsuarioId = CoAutorUsuarioId, Nome = "Co-Autor Teste Base", Url = "url.com/coautor" });
        }

        // FIX 2: Ensure Bolsista is explicitly set as active Staff.
        var bolsista = await _staffRepository.GetByUsuarioIdAsync(BolsistaUserId);
        if (bolsista == null)
        {
            await _staffRepository.AddAsync(new Staff { UsuarioId = BolsistaUserId, Nome = "Bolsista Teste", Job = FuncaoTrabalho.EditorBolsista, IsActive = true });
        }
        else
        {
            bolsista.Job = FuncaoTrabalho.EditorBolsista;
            bolsista.IsActive = true;
            await _staffRepository.UpdateAsync(bolsista);
        }

        // FIX 3: Ensure Admin has an Author record with the correct name.
        var adminAutor = await _autorRepository.GetByUsuarioIdAsync(AdminUserId);
        if (adminAutor == null)
        {
            await _autorRepository.UpsertAsync(new Autor { UsuarioId = AdminUserId, Nome = "Admin Teste Base", Url = "url.com/admin" });
        }

        // FIX 4: Ensure the new staff candidate DOES NOT exist for the creation test.
        var newStaffCandidate = await _staffRepository.GetByUsuarioIdAsync(NewStaffCandidateId);
        if (newStaffCandidate != null)
        {
            await _staffRepository.DeleteAsync(newStaffCandidate.Id);
        }

        // FIX 5: Ensure Inactive Staff is set to INACTIVE.
        var inactiveStaff = await _staffRepository.GetByUsuarioIdAsync(InactiveStaffId);
        if (inactiveStaff == null)
        {
            await _staffRepository.AddAsync(new Staff { UsuarioId = InactiveStaffId, Nome = "Staff Inativo", Job = FuncaoTrabalho.Aposentado, IsActive = false });
        }
        else
        {
            inactiveStaff.IsActive = false;
            await _staffRepository.UpdateAsync(inactiveStaff);
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

    // Teste para atualizar Posição Editorial
    [Fact]
    public async Task AtualizarMetadadosArtigoAsync_ShouldUpdatePosition_WhenAdmin()
    {
        // Arrange
        var artigo = await CreateTestArticleAsync("Artigo para Atualizar Posição", AdminUserId, null, "Admin Teste");
        var editorial = await _editorialRepository.GetByArtigoIdAsync(artigo.Id);
        Assert.Equal(PosicaoEditorial.Submetido, editorial!.Position);

        var updateInput = new UpdateArtigoMetadataInput
        {
            Posicao = PosicaoEditorial.ProntoParaPublicar
        };

        // Act
        await _artigoService.AtualizarMetadadosArtigoAsync(artigo.Id, updateInput, AdminUserId, "Testando update de posição");

        // Assert
        var updatedEditorial = await _editorialRepository.GetByArtigoIdAsync(artigo.Id);
        Assert.NotNull(updatedEditorial);
        Assert.Equal(PosicaoEditorial.ProntoParaPublicar, updatedEditorial.Position);
    }

    [Fact]
    public async Task CriarNovoStaffAsync_DeveCriarRegistroStaffCorretamente_QuandoAdmin()
    {
        // Arrange
        var requestDto = new CreateStaffRequest
        {
            UsuarioId = NewStaffCandidateId,
            Job = FuncaoTrabalho.EditorBolsista,
            Nome = "Novo Staff Candidato",
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
        Assert.Equal("Novo Staff Candidato", persistedStaff.Nome);
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
            VolumeTitulo = "Edição Pendente de Bolsista - 6",
            Edicao = 6,
            N = 2,
            Year = 2025,
            M = MesVolume.Janeiro
        };

        // Act
        var resultVolume = await _artigoService.CriarVolumeAsync(volumeInicial, BolsistaUserId, "Requisição de volume por bolsista");

        // Assert 1: Garante que NENHUM volume foi persistido.
        var persistedVolume = await _volumeRepository.GetByIdAsync(resultVolume.Id);
        Assert.Null(persistedVolume);

        // Assert 2: Garante que a pendência foi criada.
        var pendings = await _pendingRepository.BuscarPendenciaPorRequisitanteId(BolsistaUserId);
        var aPendente = pendings.FirstOrDefault(p => p.CommandType == "CreateVolume");

        Assert.NotNull(aPendente);
        Assert.Equal(TipoEntidadeAlvo.Volume, aPendente.TargetType);
        Assert.Contains("Edição Pendente de Bolsista - 6", aPendente.CommandParametersJson);
    }

    [Fact]
    public async Task AddStaffComentarioAsync_ShouldAddCommentToHistory()
    {
        // Arrange
        var artigo = await CreateTestArticleAsync("Artigo para Comentar", TestUsuarioId, null, "Autor Teste");
        var editorial = await _editorialRepository.GetByArtigoIdAsync(artigo.Id);
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
        // Assert com o nome de base que o Setup garante.
        Assert.Equal("Autor Teste Base", result.Nome);
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
            EditorIds = new List<string> { "staff_editor_id" },
            ReviewerIds = new List<string> { CoAutorUsuarioId }
        };

        // Act
        var updatedEditorial = await _artigoService.AtualizarEquipeEditorialAsync(artigo.Id, newTeam, AdminUserId, "Adicionando Revisor");

        // Assert
        Assert.NotNull(updatedEditorial);
        Assert.Contains("staff_editor_id", updatedEditorial.Team.EditorIds);
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
    // Testes de Busca (Pública)
    // =========================================================================

    [Fact]
    public async Task ObterArtigosCardListPorTituloAsync_ShouldReturnMatchingArticle()
    {
        // Arrange
        // Títulos únicos
        var artigoBusca = await CreateTestArticleAsync("Um Título Exclusivo para Busca Nova", TestUsuarioId, null, "Autor Teste");
        var artigoOutro = await CreateTestArticleAsync("Outro Artigo Totalmente Diferente", TestUsuarioId, null, "Autor Teste");

        await PublishArticleAsync(artigoBusca.Id);
        await PublishArticleAsync(artigoOutro.Id);

        // Act
        var result = await _artigoService.ObterArtigosCardListPorTituloAsync("Exclusivo para Busca Nova", 0, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Um Título Exclusivo para Busca Nova", result[0].Titulo);
    }

    [Fact]
    public async Task ObterArtigosCardListPorNomeAutorAsync_ShouldReturnMatchingArticles()
    {
        var autorTest = await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId);
        var autorCo = await _autorRepository.GetByUsuarioIdAsync(CoAutorUsuarioId);

        if (autorTest != null) await _autorRepository.DeleteAsync(autorTest.Id);
        if (autorCo != null) await _autorRepository.DeleteAsync(autorCo.Id);

        var artigoAutorReg = await CreateTestArticleAsync("Artigo Ultra-Busca 1 Z", TestUsuarioId, null, "ZWX-1-REG");
        var artigoAutorRef = await CreateTestArticleAsync("Artigo Ultra-Busca 2 Z", CoAutorUsuarioId, new List<string> { "ZWX-2-REF" }, "ZWX-3-REG");
        await PublishArticleAsync(artigoAutorReg.Id);
        await PublishArticleAsync(artigoAutorRef.Id);

        // Act 1: Search by Registered Author Name ("ZWX-1-REG")
        var resultReg = await _artigoService.ObterArtigosCardListPorNomeAutorAsync("ZWX-1-REG", 0, 10);

        // Act 2: Search by Non-Registered Author Reference ("ZWX-2-REF")
        var resultRef = await _artigoService.ObterArtigosCardListPorNomeAutorAsync("ZWX-2-REF", 0, 10);

        // Act 3: Search by Registered Author Name (Co-author, "ZWX-3-REG")
        var resultReg2 = await _artigoService.ObterArtigosCardListPorNomeAutorAsync("ZWX-3-REG", 0, 10);

        // Assert 1: Expect 1 result matching Act 1
        Assert.NotNull(resultReg);
        Assert.Single(resultReg);
        Assert.Equal(artigoAutorReg.Id, resultReg[0].Id);

        // Assert 2: Expect 1 result matching Act 2
        Assert.NotNull(resultRef);
        Assert.Single(resultRef);
        Assert.Equal(artigoAutorRef.Id, resultRef[0].Id);

        // Assert 3: Expect 1 result matching Act 3
        Assert.NotNull(resultReg2);
        Assert.Single(resultReg2);
        Assert.Equal(artigoAutorRef.Id, resultReg2[0].Id);
    }

    // =========================================================================
    // Testes de Autor
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
        var initialArticles = await _artigoService.ObterMeusArtigosCardListAsync(TestUsuarioId);
        int initialCount = initialArticles.Count;
        var artigoRascunho = await CreateTestArticleAsync("Meu Artigo Rascunho", TestUsuarioId, null, "Autor Teste Base");
        var artigoPublicado = await CreateTestArticleAsync("Meu Artigo Publicado", TestUsuarioId, null, "Autor Teste Base");
        await PublishArticleAsync(artigoPublicado.Id);

        // Act
        var result = await _artigoService.ObterMeusArtigosCardListAsync(TestUsuarioId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(initialCount + 2, result.Count);
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
        // Act
        var result = await _artigoService.ObterMeusArtigosCardListAsync(UnauthorizedUserId);
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // =========================================================================
    // Testes de Staff (Verificação e Atualização)
    // =========================================================================

    [Fact]
    public async Task VerificarStaffAsync_ShouldReturnTrue_WhenUserIsActiveStaff()
    {
        // Act
        var result = await _artigoService.VerificarStaffAsync(AdminUserId);
        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VerificarStaffAsync_ShouldReturnFalse_WhenUserIsNotStaff()
    {
        // Act
        var result = await _artigoService.VerificarStaffAsync(UnauthorizedUserId);
        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task VerificarStaffAsync_ShouldReturnFalse_WhenUserIsInactiveStaff()
    {
        // Act
        var result = await _artigoService.VerificarStaffAsync(InactiveStaffId);
        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AtualizarStaffAsync_ShouldExecuteDirectly_WhenAdminUpdatesBolsista()
    {
        // Arrange
        var updateInput = new UpdateStaffInput { UsuarioId = BolsistaUserId, Job = FuncaoTrabalho.EditorChefe };

        // Act
        var result = await _artigoService.AtualizarStaffAsync(updateInput, AdminUserId, "Promoção por Admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(FuncaoTrabalho.EditorChefe, result.Job);
        var persistedStaff = await _staffRepository.GetByUsuarioIdAsync(BolsistaUserId);
        Assert.NotNull(persistedStaff);
        Assert.Equal(FuncaoTrabalho.EditorChefe, persistedStaff.Job);
    }

    [Fact]
    public async Task AtualizarStaffAsync_ShouldCreatePendingRequest_WhenBolsistaUpdatesAdmin()
    {
        // Arrange
        var updateInput = new UpdateStaffInput { UsuarioId = AdminUserId, IsActive = false }; // Tenta aposentar o Admin

        // Act
        var result = await _artigoService.AtualizarStaffAsync(updateInput, BolsistaUserId, "Bolsista tentando aposentar Admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(FuncaoTrabalho.Administrador, result.Job);
        Assert.True(result.IsActive);
        var persistedStaff = await _staffRepository.GetByUsuarioIdAsync(AdminUserId);
        Assert.NotNull(persistedStaff);
        Assert.True(persistedStaff.IsActive);
        var pendings = await _pendingRepository.BuscarPendenciaPorRequisitanteId(BolsistaUserId);
        var aPendente = pendings.FirstOrDefault(p => p.CommandType == "UpdateStaff");
        Assert.NotNull(aPendente);
        Assert.Equal(AdminUserId, aPendente.TargetEntityId);
        Assert.Contains("\"IsActive\":false", aPendente.CommandParametersJson);
    }

    [Fact]
    public async Task ResolverRequisicaoPendenteAsync_ShouldUpdateStaff_WhenApprovingUpdateStaffCommand()
    {
        // Arrange
        // Garante que o staff inativo está inativo, se a limpeza falhar em alguma etapa.
        var inactiveStaff = await _staffRepository.GetByUsuarioIdAsync(InactiveStaffId);
        if (inactiveStaff != null && inactiveStaff.IsActive)
        {
            inactiveStaff.IsActive = false;
            await _staffRepository.UpdateAsync(inactiveStaff);
        }

        var updateInput = new UpdateStaffInput { UsuarioId = InactiveStaffId, IsActive = true }; // Tenta reinstaurar
        var staffOriginal = await _artigoService.AtualizarStaffAsync(updateInput, BolsistaUserId, "Bolsista solicita restauração");
        var pendings = await _pendingRepository.BuscarPendenciaPorRequisitanteId(BolsistaUserId);
        var pendingRequest = pendings.FirstOrDefault(p => p.CommandType == "UpdateStaff");
        Assert.NotNull(pendingRequest);

        // Act
        var success = await _artigoService.ResolverRequisicaoPendenteAsync(pendingRequest.Id, true, AdminUserId);

        // Assert
        Assert.True(success);
        var updatedStaff = await _staffRepository.GetByUsuarioIdAsync(InactiveStaffId);
        Assert.NotNull(updatedStaff);
        Assert.True(updatedStaff.IsActive); // Confirmando que o status é 'true'
        var resolvedPending = await _pendingRepository.GetByIdAsync(pendingRequest.Id);
        Assert.NotNull(resolvedPending);
        Assert.Equal(StatusPendente.Aprovado, resolvedPending.Status);
    }

    // Teste para Resolver Posição de Metadados Pendente
    [Fact]
    public async Task ResolverRequisicaoPendenteAsync_ShouldUpdatePosition_WhenApprovingUpdateArtigoMetadata()
    {
        // Arrange
        var artigo = await CreateTestArticleAsync("Artigo para Posição Pendente", BolsistaUserId, null, "Bolsista Teste");
        var updateInput = new UpdateArtigoMetadataInput { Posicao = PosicaoEditorial.ProntoParaPublicar };

        // 1. Bolsista cria a pendência
        await _artigoService.AtualizarMetadadosArtigoAsync(artigo.Id, updateInput, BolsistaUserId, "Solicitando 'ProntoParaPublicar'");

        var pendings = await _pendingRepository.BuscarPendenciaPorRequisitanteId(BolsistaUserId);
        var pendingRequest = pendings.FirstOrDefault(p => p.CommandType == "UpdateArtigoMetadata");
        Assert.NotNull(pendingRequest);

        var editorialOriginal = await _editorialRepository.GetByArtigoIdAsync(artigo.Id);
        Assert.Equal(PosicaoEditorial.Submetido, editorialOriginal!.Position); // Confirma estado inicial

        // 2. Admin aprova
        // Act
        var success = await _artigoService.ResolverRequisicaoPendenteAsync(pendingRequest.Id, true, AdminUserId);

        // Assert
        Assert.True(success);
        var updatedEditorial = await _editorialRepository.GetByArtigoIdAsync(artigo.Id);
        Assert.NotNull(updatedEditorial);
        Assert.Equal(PosicaoEditorial.ProntoParaPublicar, updatedEditorial.Position); // Confirma mudança
        Assert.Equal(PosicaoEditorial.ProntoParaPublicar, updatedEditorial.Position);
    }

    // =========================================================================
    //  Testes de Busca Editorial (Staff)
    // =========================================================================

    [Fact]
    public async Task ObterArtigosEditorialPorTipoAsync_ShouldReturnAllStatuses()
    {
        // Arrange
        var artigoBlogRascunho = await CreateTestArticleAsync("Blog Rascunho Tipo Staff", TestUsuarioId, null, "Autor Teste");
        artigoBlogRascunho.Tipo = TipoArtigo.Blog;
        await _artigoService.AtualizarMetadadosArtigoAsync(artigoBlogRascunho.Id, new UpdateArtigoMetadataInput { Tipo = TipoArtigo.Blog }, AdminUserId, "Definindo tipo");

        var artigoBlogPublicado = await CreateTestArticleAsync("Blog Publicado Tipo Staff", TestUsuarioId, null, "Autor Teste");
        artigoBlogPublicado.Tipo = TipoArtigo.Blog;
        await _artigoService.AtualizarMetadadosArtigoAsync(artigoBlogPublicado.Id, new UpdateArtigoMetadataInput { Tipo = TipoArtigo.Blog, Status = StatusArtigo.Publicado }, AdminUserId, "Definindo tipo e publicando");

        // Act
        var result = await _artigoService.ObterArtigosEditorialPorTipoAsync(TipoArtigo.Blog, 0, 10, AdminUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Deve encontrar ambos
        Assert.Contains(result, a => a.Id == artigoBlogRascunho.Id);
        Assert.Contains(result, a => a.Id == artigoBlogPublicado.Id);
    }

    [Fact]
    public async Task SearchArtigosEditorialByTitleAsync_ShouldReturnAllStatuses()
    {
        // Arrange
        var artigoRascunho = await CreateTestArticleAsync("TituloUnicoStaffRascunho", TestUsuarioId, null, "Autor Teste");
        var artigoPublicado = await CreateTestArticleAsync("TituloUnicoStaffPublicado", TestUsuarioId, null, "Autor Teste");
        await PublishArticleAsync(artigoPublicado.Id);

        // Act
        var resultRascunho = await _artigoService.SearchArtigosEditorialByTitleAsync("TituloUnicoStaffRascunho", 0, 10, AdminUserId);
        var resultPublicado = await _artigoService.SearchArtigosEditorialByTitleAsync("TituloUnicoStaffPublicado", 0, 10, AdminUserId);

        // Assert
        Assert.Single(resultRascunho);
        Assert.Equal(artigoRascunho.Id, resultRascunho[0].Id);
        Assert.Single(resultPublicado);
        Assert.Equal(artigoPublicado.Id, resultPublicado[0].Id);
    }

    [Fact]
    public async Task SearchArtigosEditorialByAutorIdsAsync_ShouldReturnAllStatuses()
    {
        // Arrange
        var autor = await _autorRepository.GetByUsuarioIdAsync(CoAutorUsuarioId); // Usa um autor limpo
        Assert.NotNull(autor);

        var artigoRascunho = await CreateTestArticleAsync("Artigo Rascunho CoAutor Staff", CoAutorUsuarioId, null, "Co-Autor Teste");
        var artigoPublicado = await CreateTestArticleAsync("Artigo Publicado CoAutor Staff", CoAutorUsuarioId, null, "Co-Autor Teste");
        await PublishArticleAsync(artigoPublicado.Id);

        var autorIds = new List<string> { autor.Id };

        // Act
        var result = await _artigoService.SearchArtigosEditorialByAutorIdsAsync(autorIds, 0, 10, AdminUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Deve encontrar ambos
    }

    [Fact]
    public async Task SearchArtigosEditorial_ShouldThrowUnauthorized_WhenUserIsNotStaff()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _artigoService.ObterArtigosEditorialPorTipoAsync(TipoArtigo.Artigo, 0, 10, UnauthorizedUserId)
        );

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _artigoService.SearchArtigosEditorialByTitleAsync("teste", 0, 10, UnauthorizedUserId)
        );

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _artigoService.SearchArtigosEditorialByAutorIdsAsync(new List<string> { "id" }, 0, 10, UnauthorizedUserId)
        );
    }
}