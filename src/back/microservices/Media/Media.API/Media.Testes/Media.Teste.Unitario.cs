using Xunit;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Media.Intf.Models;
using Media.DbContext;
using Media.Server.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class MediaServiceTests
{
    private readonly Mock<MongoDbContext> _mockContext;
    private readonly Mock<IMongoCollection<Midia>> _mockCollection;
    private readonly MediaService _service;
    private readonly ObjectId _objectId = ObjectId.GenerateNewId();
    private readonly string _hexId;
    private readonly string _hexOrigem;

    public MediaServiceTests() // Montamos o serviço moq para o banco de dados de teste
    {
        _hexId = _objectId.ToString();
        _hexOrigem = _objectId.ToString();
        var midia = new Midia
        {
            Id = _objectId,
            Origem = _objectId,
            Tipo = "TestTipo",
            Url = "http://test.url"
        };
        var midiaList = new List<Midia> { midia };

        _mockCollection = new Mock<IMongoCollection<Midia>>();
        _mockContext = new Mock<MongoDbContext>(null);


        _mockContext.Setup(c => c.Midias).Returns(_mockCollection.Object);

        var mockFindAll = MongoMoqHelper.MockFindFluent(midiaList);
        _mockCollection
            .Setup(c => c.Find(It.IsAny<FilterDefinition<Midia>>(), It.IsAny<FindOptions>()))
            .Returns(mockFindAll.Object);

        _mockCollection
            .Setup(c => c.Find(It.IsAny<FilterDefinition<Midia>>(), It.IsAny<FindOptions>()))
            .Returns<FilterDefinition<Midia>, FindOptions>((filter, options) =>
            {
                var findFluent = MongoMoqHelper.MockFindFluent(midiaList);
                return findFluent.Object;
            });

        _service = new MediaService(_mockContext.Object);
    }


    [Fact]
    public async Task GetMediaAsync_All_RetornaListaDtoComHexString()
    {
        // Inicia o query
        var result = await _service.GetMediaAsync();

        // Confere se recebe algo e o que é esse algo
        Assert.NotNull(result);
        Assert.Single(result);
        var dto = result.First();

        // Verifica se o Id é uma string Hexadecimal
        Assert.Equal(_hexId, dto.Id);
        // Caso não tenha aviso de erro até aqui é porque o teste deu certo
    }

    [Fact]
    public async Task GetMediaAsync_UsandoUmaIdStringERecebendoUmaString() { 
        
        // Durante a preparação da operação get o moq já criou nossa lista de itens
        // Inicia o teste
        var result = await _service.GetMediaAsync(_hexId);

        // Confere o que recebe
        Assert.NotNull(result);

        // Confirma que recebeu uma string Hexadecimal
        Assert.Equal(_hexId, result.Id);
        // Caso não tenha aviso de erro até aqui é porque o teste deu certo
    }

    [Fact]
    public async Task GetMediaAsync_ComUmaStringInvalida_DeveVirNulo()
    {
        // Montamos uma id com string invalida
        string invalidHex = "this-is-not-a-valid-id";

        // Iniciamos o teste
        var result = await _service.GetMediaAsync(invalidHex);

        // Alogica do serviço deve resolver quando for invalida e retornar nulo
        Assert.Null(result);
        // Caso seja identificada como nulo o teste deu certo
    }


    [Fact]
    public async Task UpdateMediaAsync_UsandoIdStringValida()
    {
        // Montamos o que vai ser enviado 
        var updatedMidia = new Midia { Origem = _objectId, Tipo = "NewTipo", Url = "new.url" };

        // Usamos o Moq para simular o banco e testar a lógica usada no comando Find
        _mockCollection
            .Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Midia>>(),
                It.IsAny<Midia>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<ReplaceOneResult>().Object); 
        // Retorna um resultado válido

        // Iniciamos o teste
        await _service.UpdateMediaAsync(_hexId, updatedMidia);

        // Verificamos se o metodo Replace foi executado exatamente uma vez
        _mockCollection.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Midia>>(),
            It.Is<Midia>(m => m.Id == _objectId), 
            // Verificamos o modelo interno de midia
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);

        // Confirma tambem se o valor id foi colocado corretamente durante o serviço
        Assert.Equal(_objectId, updatedMidia.Id);
        // Como esse metodo não tem retorno se não houver mensagem de erro é porque deu certo
    }

    [Fact]
    public async Task UpdateMediaAsync_UsandoIdInvalida_DeveRetornarAvisoDeErro()
    {
        // Montamos o modelo para teste e a id inválida
        string invalidHex = "invalid-hex";
        var updatedMidia = new Midia
        {
            Id = _objectId,
            Origem = _objectId,
            Tipo = "TestTipo",
            Url = "http://test.url"
        };

        // Iniciamos o teste como uma execessão para ver se é pego pelo trypass
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateMediaAsync(invalidHex, updatedMidia));
        // Como o metodo não tem retorno se ele informou erro neste ponto é porque deu certo o teste
    }

    [Fact]
    public async Task DeleteMediaAsync_UsandoIdValida()
    {
        // Montamos a coleção moq
        _mockCollection
            .Setup(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Midia>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<DeleteResult>().Object); 

        // Iniciamos o teste
        await _service.DeleteMediaAsync(_hexId);

        // Conferimos se o metodo Delete foi executado exatamente uma vez
        _mockCollection.Verify(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Midia>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        // Como esse metodo não tem retorno se não houver mensagem de erro é porque deu certo
    }

    [Fact]
    public async Task DeleteMediaAsync_UsandoIdInvalida_DeveRetornarErro()
    {
        // Montamos a id inválida
        string invalidHex = "invalid-hex";

        // Iniciamos o teste
        await _service.DeleteMediaAsync(invalidHex);

        // Verificamos que o metodo DeleteOneAsync nunca foi executado por não ter passado no trypass
        _mockCollection.Verify(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Midia>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        // Como o metodo não tem retorno se ele informou erro neste ponto é porque deu certo o teste
    }
}