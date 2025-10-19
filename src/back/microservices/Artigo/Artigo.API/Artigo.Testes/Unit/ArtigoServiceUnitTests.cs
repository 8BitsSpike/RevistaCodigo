using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.Services;
using AutoMapper;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System; // Necessário para Assert.ThrowsAsync<UnauthorizedAccessException>

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
        private const string AdminUserId = "user_admin_01";
        private const string EditorChefeUserId = "user_chefe_01";
        private const string EditorBolsistaUserId = "user_bolsista_01";
        private const string NewStaffUserId = "user_novo_104";


        public ArtigoServiceUnitTests()
        {
            // Inicialização dos Mocks
            _mockArtigoRepo = new Mock<IArtigoRepository>();
            _mockStaffRepo = new Mock<IStaffRepository>();
            _mockEditorialRepo = new Mock<IEditorialRepository>();
            var mockMapper = new Mock<IMapper>();

            // Configurações Comuns de Mocks de Staff para Testes de Permissão
            // Nota: Retornamos null para o NewStaffUserId para simular que ele ainda não existe.
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(NewStaffUserId)).ReturnsAsync((Staff?)null);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId)).ReturnsAsync(new Staff { UsuarioId = AdminUserId, Job = FuncaoTrabalho.Administrador });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorChefeUserId)).ReturnsAsync(new Staff { UsuarioId = EditorChefeUserId, Job = FuncaoTrabalho.EditorChefe });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId)).ReturnsAsync(new Staff { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorBolsista });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(UnauthorizedUserId)).ReturnsAsync((Staff?)null);

            // Instancia o ArtigoService injetando os mocks (outros mocks não são necessários para este teste)
            _artigoService = new ArtigoService(
                _mockArtigoRepo.Object,
                new Mock<IAutorRepository>().Object,
                _mockStaffRepo.Object,
                _mockEditorialRepo.Object,
                new Mock<IArtigoHistoryRepository>().Object,
                new Mock<IPendingRepository>().Object,
                new Mock<IInteractionRepository>().Object,
                new Mock<IVolumeRepository>().Object,
                new Mock<Artigo.Server.Interfaces.IExternalUserService>().Object, // Adicionado IExternalUserService Mock
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
                Status = StatusArtigo.Rascunho,
                EditorialId = "editorial_1"
            };

            var staffRecord = new Staff
            {
                UsuarioId = AuthorizedStaffId,
                Job = FuncaoTrabalho.EditorBolsista
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
                () => _artigoService.AtualizarMetadadosArtigoAsync(unauthorizedArtigoUpdate, UnauthorizedUserId)
            );
        }

        [Fact]
        public async Task UpdateArtigoMetadataAsync_ShouldSucceed_WhenUserIsOnEditorialTeam()
        {
            // Arrange
            var draftArtigo = new Artigo.Intf.Entities.Artigo
            {
                Id = TestArtigoId,
                Status = StatusArtigo.Rascunho,
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
                Job = FuncaoTrabalho.EditorBolsista
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
            var result = await _artigoService.AtualizarMetadadosArtigoAsync(authorizedArtigoUpdate, AuthorizedStaffId);

            // Assert
            Assert.True(result);
            // Verifica se o método UpdateAsync do repositório foi chamado uma vez, confirmando a execução da lógica.
            _mockArtigoRepo.Verify(r => r.UpdateAsync(It.Is<Artigo.Intf.Entities.Artigo>(
                a => a.Titulo == "Titulo Atualizado"
            )), Times.Once);
        }

        // =========================================================================
        // NOVOS TESTES PARA STAFF MANAGEMENT (FIX 33)
        // =========================================================================

        [Theory]
        [InlineData(EditorBolsistaUserId)]
        [InlineData(UnauthorizedUserId)] // Não-Staff
        public async Task CriarNovoStaffAsync_ShouldThrowUnauthorizedException_WhenNotAdminOrEditorChefe(string currentUserId)
        {
            // Arrange
            var newJob = FuncaoTrabalho.EditorBolsista;

            // Simula que o currentUserId não é Admin nem EditorChefe
            // O mock já está configurado no construtor para esses IDs

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _artigoService.CriarNovoStaffAsync(NewStaffUserId, newJob, currentUserId)
            );

            // Verifica que o método AddAsync do repositório NUNCA foi chamado
            _mockStaffRepo.Verify(r => r.AddAsync(It.IsAny<Staff>()), Times.Never);
        }

        [Theory]
        [InlineData(AdminUserId, FuncaoTrabalho.EditorBolsista)]
        [InlineData(EditorChefeUserId, FuncaoTrabalho.Aposentado)]
        public async Task CriarNovoStaffAsync_ShouldSucceed_WhenAdminOrEditorChefe(string currentUserId, FuncaoTrabalho newJob)
        {
            // Arrange: Novo Staff não existe (configurado no construtor)
            var createdStaff = new Staff(); // Objeto para capturar a entidade criada

            _mockStaffRepo.Setup(r => r.AddAsync(It.IsAny<Staff>()))
                .Callback<Staff>(s => {
                    // Captura a entidade que foi passada para o repositório
                    createdStaff.UsuarioId = s.UsuarioId;
                    createdStaff.Job = s.Job;
                    createdStaff.IsActive = s.IsActive;
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _artigoService.CriarNovoStaffAsync(NewStaffUserId, newJob, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NewStaffUserId, createdStaff.UsuarioId);
            Assert.Equal(newJob, createdStaff.Job);
            Assert.True(createdStaff.IsActive);

            // Verifica se o método AddAsync do repositório foi chamado uma vez
            _mockStaffRepo.Verify(r => r.AddAsync(It.IsAny<Staff>()), Times.Once);
        }

        [Fact]
        public async Task CriarNovoStaffAsync_ShouldThrowInvalidOperationException_WhenStaffAlreadyExists()
        {
            // Arrange
            var existingStaffId = EditorBolsistaUserId; // Um usuário que já é Staff

            // Simula que o usuário já existe
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(existingStaffId))
                .ReturnsAsync(new Staff { UsuarioId = existingStaffId, Job = FuncaoTrabalho.EditorBolsista });

            // Act & Assert: Tentativa de criar um Staff usando um Administrador
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _artigoService.CriarNovoStaffAsync(existingStaffId, FuncaoTrabalho.EditorChefe, AdminUserId)
            );

            // Verifica que o repositório NÃO foi chamado para adicionar um novo registro
            _mockStaffRepo.Verify(r => r.AddAsync(It.IsAny<Staff>()), Times.Never);
        }
    }
}