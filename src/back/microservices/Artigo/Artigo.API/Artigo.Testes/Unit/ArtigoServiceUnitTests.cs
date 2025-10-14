using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.Services;
using AutoMapper;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Artigo.Testes.Unit
{
    public class ArtigoServiceUnitTests
    {
        private readonly Mock<IArtigoRepository> _mockArtigoRepo;
        private readonly Mock<IStaffRepository> _mockStaffRepo;
        private readonly Mock<IEditorialRepository> _mockEditorialRepo;
        private readonly ArtigoService _artigoService;

        private const string TestArtigoId = "artigo_100";
        private const string UnauthorizedUserId = "user_999";
        private const string AuthorizedStaffId = "user_101";

        public ArtigoServiceUnitTests()
        {
            // Inicialização dos Mocks
            _mockArtigoRepo = new Mock<IArtigoRepository>();
            _mockStaffRepo = new Mock<IStaffRepository>();
            _mockEditorialRepo = new Mock<IEditorialRepository>();
            var mockMapper = new Mock<IMapper>();

            // Instancia o ArtigoService injetando os mocks (outros mocks não são necessários para este teste)
            _artigoService = new ArtigoService(
    // 1. ArtigoRepository
    _mockArtigoRepo.Object,
    // 2. AutorRepository
    new Mock<IAutorRepository>().Object,
    // 3. StaffRepository
    _mockStaffRepo.Object,
    // 4. EditorialRepository
    _mockEditorialRepo.Object,
    // 5. ArtigoHistoryRepository
    new Mock<IArtigoHistoryRepository>().Object,
    // 6. PendingRepository
    new Mock<IPendingRepository>().Object,
    // 7. InteractionRepository
    new Mock<IInteractionRepository>().Object,
    // 8. VolumeRepository (Added Mock)
    new Mock<IVolumeRepository>().Object,
    // 9. IMapper
    mockMapper.Object
);
        }

        [Fact]
        public async Task UpdateArtigoMetadataAsync_ShouldThrowUnauthorizedException_WhenUserIsNotStaffOrAuthor()
        {
            // Arrange
            var draftArtigo = new Artigo.Intf.Entities.Artigo
            {
                Id = TestArtigoId,
                Status = ArtigoStatus.Draft,
                EditorialId = "editorial_1"
            };

            var staffRecord = new Staff
            {
                UsuarioId = AuthorizedStaffId,
                Job = JobRole.EditorBolsista
            };

            var editorialRecord = new Editorial
            {
                Id = "editorial_1",
                Team = new EditorialTeam { InitialAuthorId = new List<string> { "user_102" } }
            };

            // Setup Mocks para simular o cenário de falha de autorização
            _mockArtigoRepo.Setup(r => r.GetByIdAsync(TestArtigoId)).ReturnsAsync(draftArtigo);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(UnauthorizedUserId)).ReturnsAsync(staffRecord);
            _mockEditorialRepo.Setup(r => r.GetByIdAsync("editorial_1")).ReturnsAsync(editorialRecord);
            _mockArtigoRepo.Setup(r => r.UpdateAsync(It.IsAny<Artigo.Intf.Entities.Artigo>())).ReturnsAsync(true);

            var unauthorizedArtigoUpdate = new Artigo.Intf.Entities.Artigo { Id = TestArtigoId, Titulo = "Novo Titulo" };

            // Act & Assert
            // O serviço deve lançar uma exceção de não-autorizado, pois UnauthorizedUserId não está na equipe editorial.
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _artigoService.UpdateArtigoMetadataAsync(unauthorizedArtigoUpdate, UnauthorizedUserId)
            );
        }

        [Fact]
        public async Task UpdateArtigoMetadataAsync_ShouldSucceed_WhenUserIsOnEditorialTeam()
        {
            // Arrange
            var draftArtigo = new Artigo.Intf.Entities.Artigo
            {
                Id = TestArtigoId,
                Status = ArtigoStatus.Draft,
                EditorialId = "editorial_1",
                Titulo = "Titulo Antigo"
            };

            var authorizedArtigoUpdate = new Artigo.Intf.Entities.Artigo
            {
                Id = TestArtigoId,
                Titulo = "Titulo Atualizado"
            };

            var staffRecord = new Staff
            {
                UsuarioId = AuthorizedStaffId,
                Job = JobRole.EditorBolsista
            };

            var editorialRecord = new Editorial
            {
                Id = "editorial_1",
                // Coloca o AuthorizedStaffId na equipe de revisores
                Team = new EditorialTeam { ReviewerIds = new List<string> { AuthorizedStaffId } }
            };

            // Setup Mocks
            _mockArtigoRepo.Setup(r => r.GetByIdAsync(TestArtigoId)).ReturnsAsync(draftArtigo);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AuthorizedStaffId)).ReturnsAsync(staffRecord);
            _mockEditorialRepo.Setup(r => r.GetByIdAsync("editorial_1")).ReturnsAsync(editorialRecord);
            _mockArtigoRepo.Setup(r => r.UpdateAsync(It.IsAny<Artigo.Intf.Entities.Artigo>())).ReturnsAsync(true);

            // Act
            var result = await _artigoService.UpdateArtigoMetadataAsync(authorizedArtigoUpdate, AuthorizedStaffId);

            // Assert
            Assert.True(result);
            // Verifica se o método UpdateAsync do repositório foi chamado uma vez, confirmando a execução da lógica.
            _mockArtigoRepo.Verify(r => r.UpdateAsync(It.Is<Artigo.Intf.Entities.Artigo>(
                a => a.Titulo == "Titulo Atualizado" // Verifica se o título foi atualizado no objeto passado ao repositório
            )), Times.Once);
        }
    }
}