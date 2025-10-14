using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using Artigo.Testes.Integration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

// Define a collection para que o Fixture seja inicializado apenas uma vez
[CollectionDefinition("ArtigoServiceIntegration")]
public class ArtigoServiceIntegrationCollection : ICollectionFixture<ArtigoIntegrationTestFixture> { }

[Collection("ArtigoServiceIntegration")]
public class ArtigoServiceIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IArtigoService _artigoService;
    private readonly IEditorialRepository _editorialRepository;
    private readonly IArtigoHistoryRepository _historyRepository;
    private readonly IAutorRepository _autorRepository;

    private const string TestUsuarioId = "test_user_400";
    private const string ArticleContent = "Conteúdo completo do Artigo de Teste.";
    private const string ArtigoContent = "Conteúdo completo do Artigo de Teste.";

    public ArtigoServiceIntegrationTests(ArtigoIntegrationTestFixture fixture)
    {
        _serviceProvider = fixture.ServiceProvider;
        // Obtém o Service Provider para resolver as dependências
        _artigoService = _serviceProvider.GetRequiredService<IArtigoService>();
        _editorialRepository = _serviceProvider.GetRequiredService<IEditorialRepository>();
        _historyRepository = _serviceProvider.GetRequiredService<IArtigoHistoryRepository>();
        _autorRepository = _serviceProvider.GetRequiredService<IAutorRepository>();
    }

    [Fact]
    public async Task CreateArtigoAsync_DeveCriarArtigoEditorialEHistoricoCorretamente()
    {
        // Arrange
        var requestArtigo = new Artigo.Intf.Entities.Artigo
        {
            Titulo = "Teste de Integração de Artigo",
            Resumo = "Este é um artigo criado via teste de integração.",
            Tipo = ArtigoTipo.Artigo
        };

        // Act
        var createdArtigo = await _artigoService.CreateArtigoAsync(requestArtigo, ArtigoContent, TestUsuarioId);

        // Assert 1: Artigo Core deve ser criado corretamente
        Assert.NotNull(createdArtigo);
        Assert.Equal(requestArtigo.Titulo, createdArtigo.Titulo);
        Assert.Equal(ArtigoStatus.Draft, createdArtigo.Status);
        Assert.NotEmpty(createdArtigo.EditorialId);
        Assert.Contains(TestUsuarioId, createdArtigo.AutorIds);

        // Assert 2: ArtigoHistory deve existir e conter o conteúdo correto
        var editorial = await _editorialRepository.GetByIdAsync(createdArtigo.EditorialId);
        Assert.NotNull(editorial);
        Assert.NotEmpty(editorial.CurrentHistoryId);
        Assert.Equal(EditorialPosition.Submitted, editorial.Position);
        Assert.Contains(editorial.CurrentHistoryId, editorial.HistoryIds);

        var history = await _historyRepository.GetByIdAsync(editorial.CurrentHistoryId);
        Assert.NotNull(history);
        Assert.Equal(createdArtigo.Id, history.ArtigoId);
        Assert.Equal(ArtigoContent, history.Content);
        Assert.Equal(ArtigoVersion.Original, history.Version);

        // Assert 3: Registro de Autor deve ser criado/atualizado
        var autor = await _autorRepository.GetByUsuarioIdAsync(TestUsuarioId);
        Assert.NotNull(autor);
        Assert.Contains(createdArtigo.Id, autor.ArtigoWorkIds);
        Assert.Contains(autor.Contribuicoes, c => c.ArtigoId == createdArtigo.Id && c.Role == ContribuicaoRole.AutorPrincipal);
    }
}