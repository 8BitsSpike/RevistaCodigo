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
using Artigo.Intf.Inputs; // *** ADICIONADO ***

// Define a collection para que o Fixture seja inicializado apenas uma vez
[CollectionDefinition("ArtigoServiceIntegration")]
public class ArtigoServiceIntegrationCollection : ICollectionFixture<ArtigoIntegrationTestFixture> { }

[Collection("ArtigoServiceIntegration")]
// *** ATUALIZADO: Implementa IAsyncLifetime e IDisposable ***
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
    private const string TestCommentary = "Comentário de teste de integração";

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

        // *** REMOVIDO: SetupTestUsers().GetAwaiter().GetResult(); ***
    }

    // *** NOVO: Método de setup assíncrono do xUnit ***
    public async Task InitializeAsync()
    {
        // Este método é chamado após o construtor e antes de cada teste
        await SetupTestUsers();
    }

    // *** NOVO: Implementação vazia, Dispose fará a limpeza ***
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task SetupTestUsers()
    {
        if (await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId) == null)
        {
            await _autorRepository.UpsertAsync(new Autor { UsuarioId = TestUsuarioId, Nome = "Autor Teste", Url = "url.com/autor" });
        }
        if (await _autorRepository.GetByUsuarioIdAsync(CoAutorUsuarioId) == null)
        {
            await _autorRepository.UpsertAsync(new Autor { UsuarioId = CoAutorUsuarioId, Nome = "Co-Autor Teste", Url = "url.com/coautor" });
        }
        if (await _staffRepository.GetByUsuarioIdAsync(BolsistaUserId) == null)
        {
            await _staffRepository.AddAsync(new Staff { UsuarioId = BolsistaUserId, Nome = "Bolsista Teste", Job = FuncaoTrabalho.EditorBolsista });
        }
    }

    public void Dispose()
    {
        _scope.Dispose();
    }

    [Fact]
    public async Task CreateArtigoAsync_DeveCriarArtigoEAutoresCorretamente()
    {
        // Arrange
        // *** CORREÇÃO: Mapeamento de DTO movido para simular a camada de API ***
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
            Autores = autoresInput, // Usa a lista de AutorInputDTO
            ReferenciasAutor = new List<string> { "Referencia Externa" }
        };

        // Mapeamento que ocorreria na API/Mutations
        var newArtigo = _mapper.Map<Artigo.Intf.Entities.Artigo>(requestDto);
        var autores = _mapper.Map<List<Autor>>(requestDto.Autores);

        // Act
        var createdArtigo = await _artigoService.CreateArtigoAsync(newArtigo, requestDto.Conteudo, autores, TestUsuarioId, TestCommentary);

        // Assert
        Assert.NotNull(createdArtigo);
        Assert.Equal(requestDto.Titulo, createdArtigo.Titulo);
        var autor1 = await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId);
        var autor2 = await _autorRepository.GetByUsuarioIdAsync(CoAutorUsuarioId);
        Assert.NotNull(autor1);
        Assert.NotNull(autor2);
        Assert.Equal("Autor Teste", autor1.Nome);
        Assert.Contains(autor1.Id, createdArtigo.AutorIds);
        var editorial = await _editorialRepository.GetByIdAsync(createdArtigo.EditorialId);
        Assert.NotNull(editorial);
        var history = await _historyRepository.GetByIdAsync(editorial.CurrentHistoryId);
        Assert.NotNull(history);
        Assert.Equal(ArticleContent, history.Content);
        Assert.Contains(createdArtigo.Id, autor1.ArtigoWorkIds);
    }

    [Fact]
    public async Task CriarNovoStaffAsync_DeveCriarRegistroStaffCorretamente_QuandoAdmin()
    {
        // Arrange
        // *** CORREÇÃO: Mapeamento de DTO movido para simular a camada de API ***
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
        Assert.Equal("Novo Staff", newStaff.Nome);
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
            Year = 2024,
            M = MesVolume.Marco
            // *** CORREÇÃO: Mapeamento de DTO não é necessário aqui, a entidade é passada ***
        };

        // Act
        var createdVolume = await _artigoService.CriarVolumeAsync(volumeInicial, AdminUserId, TestCommentary);

        // Assert
        Assert.NotNull(createdVolume);
        Assert.NotEmpty(createdVolume.Id);
        var persistedVolume = await _volumeRepository.GetByIdAsync(createdVolume.Id);
        Assert.NotNull(persistedVolume);
        Assert.Equal("Edição Especial de Verão", persistedVolume.VolumeTitulo);
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

        // Assert 1: O volume NÃO deve existir no repositório
        var persistedVolume = await _volumeRepository.GetByIdAsync(resultVolume.Id); // ID estará vazio
        Assert.Null(persistedVolume);

        // Assert 2: Uma requisição PENDING deve ter sido criada
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
        // *** CORREÇÃO: Mapeamento de DTO movido para simular a camada de API ***
        var requestDto = new CreateArtigoRequest
        {
            Titulo = "Artigo para Comentar",
            Conteudo = "Conteúdo",
            Autores = new List<AutorInputDTO>
            {
                new AutorInputDTO { UsuarioId = TestUsuarioId, Nome = "Autor Teste", Url = "url.com/autor" }
            }
        };
        var newArtigo = _mapper.Map<Artigo.Intf.Entities.Artigo>(requestDto);
        var autores = _mapper.Map<List<Autor>>(requestDto.Autores);
        var artigo = await _artigoService.CreateArtigoAsync(newArtigo, requestDto.Conteudo, autores, TestUsuarioId, TestCommentary);
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

    // *** NOVO TESTE DE INTEGRAÇÃO ***
    [Fact]
    public async Task ObterAutorCardAsync_ShouldReturnAutor()
    {
        // Arrange
        // O autor já foi criado no SetupTestUsers (InitializeAsync)
        var autor = await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId);
        Assert.NotNull(autor); // Garante que o setup funcionou

        // Act
        var result = await _artigoService.ObterAutorCardAsync(autor.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestUsuarioId, result.UsuarioId);
        Assert.Equal("Autor Teste", result.Nome);
    }

    // *** NOVO TESTE DE INTEGRAÇÃO ***
    [Fact]
    public async Task ObterVolumesListAsync_ShouldReturnVolumes()
    {
        // Arrange
        // Cria um volume para garantir que a lista não esteja vazia
        var volume = new Volume { VolumeTitulo = "Volume de Teste para Lista", Edicao = 1, N = 1, Year = 2025, M = MesVolume.Maio };
        await _artigoService.CriarVolumeAsync(volume, AdminUserId, "Teste de lista");

        // Act
        var result = await _artigoService.ObterVolumesListAsync(0, 10);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, v => v.VolumeTitulo == "Volume de Teste para Lista");
    }
}