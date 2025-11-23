using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.Services;
using AutoMapper;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System;
using Artigo.Intf.Inputs;
using System.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace Artigo.Testes.Unit
{
    public class ArtigoServiceUnitTests
    {
        private readonly Mock<IArtigoRepository> _mockArtigoRepo;
        private readonly Mock<IStaffRepository> _mockStaffRepo;
        private readonly Mock<IEditorialRepository> _mockEditorialRepo;
        private readonly Mock<IAutorRepository> _mockAutorRepo;
        private readonly Mock<IVolumeRepository> _mockVolumeRepo;
        private readonly Mock<IArtigoHistoryRepository> _mockHistoryRepo;
        private readonly Mock<IPendingRepository> _mockPendingRepo;
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;

        private readonly ArtigoService _artigoService;

        // --- Constantes de Teste ---
        private const string TestArtigoId = "artigo_100";
        private const string TestEditorialId = "editorial_100";
        private const string UnauthorizedUserId = "user_999";
        private const string AuthorizedStaffId = "user_101"; // Usado para testes genéricos de staff
        private const string AdminUserId = "user_admin_01";
        private const string EditorChefeUserId = "user_chefe_01";
        private const string EditorBolsistaUserId = "user_bolsista_01";
        private const string NewStaffUserId = "user_novo_104";
        private const string TestAutorId = "autor_local_001";
        private const string TestAutorUsuarioId = "user_autor_001";
        private const string TestVolumeId = "volume_local_002";
        private const string TestHistoryId = "history_001";
        private const string TestCommentary = "Teste de comentário";
        private const string InactiveStaffId = "user_inactive_02";

        private readonly object _sessionHandle = new Mock<MongoDB.Driver.IClientSessionHandle>().Object;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // --- Configuração (Construtor) ---
        public ArtigoServiceUnitTests()
        {
            _mockArtigoRepo = new Mock<IArtigoRepository>();
            _mockStaffRepo = new Mock<IStaffRepository>();
            _mockEditorialRepo = new Mock<IEditorialRepository>();
            _mockAutorRepo = new Mock<IAutorRepository>();
            _mockVolumeRepo = new Mock<IVolumeRepository>();
            _mockHistoryRepo = new Mock<IArtigoHistoryRepository>();
            _mockPendingRepo = new Mock<IPendingRepository>();
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            // --- Mocking do Staff Repository para todas as chamadas de Staff/Auth ---
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, It.IsAny<object>())).ReturnsAsync(new Staff { UsuarioId = AdminUserId, Job = FuncaoTrabalho.Administrador, IsActive = true });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorChefeUserId, It.IsAny<object>())).ReturnsAsync(new Staff { UsuarioId = EditorChefeUserId, Job = FuncaoTrabalho.EditorChefe, IsActive = true });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId, It.IsAny<object>())).ReturnsAsync(new Staff { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorBolsista, IsActive = true });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(InactiveStaffId, It.IsAny<object>())).ReturnsAsync(new Staff { UsuarioId = InactiveStaffId, Job = FuncaoTrabalho.Aposentado, IsActive = false });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AuthorizedStaffId, It.IsAny<object>())).ReturnsAsync(new Staff { UsuarioId = AuthorizedStaffId, Job = FuncaoTrabalho.EditorChefe, IsActive = true });

            // Garante que usuários não-staff ou não-autores retornem null
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(UnauthorizedUserId, It.IsAny<object>())).ReturnsAsync((Staff?)null);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(TestAutorUsuarioId, It.IsAny<object>())).ReturnsAsync((Staff?)null);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(NewStaffUserId, It.IsAny<object>())).ReturnsAsync((Staff?)null);
            // --------------------------------------------------------------------------

            _mockAutorRepo.Setup(r => r.GetByIdAsync(TestAutorId, It.IsAny<object>())).ReturnsAsync(new Autor { Id = TestAutorId, UsuarioId = TestAutorUsuarioId });
            _mockVolumeRepo.Setup(r => r.GetByIdAsync(TestVolumeId, It.IsAny<object>())).ReturnsAsync(new Volume { Id = TestVolumeId });
            _mockHistoryRepo.Setup(r => r.GetByIdAsync(TestHistoryId, It.IsAny<object>())).ReturnsAsync(new ArtigoHistory { Id = TestHistoryId });

            _mockUow.Setup(u => u.GetSessionHandle()).Returns(_sessionHandle);

            _artigoService = new ArtigoService(
                _mockUow.Object,
                _mockArtigoRepo.Object,
                _mockAutorRepo.Object,
                _mockStaffRepo.Object,
                _mockEditorialRepo.Object,
                _mockHistoryRepo.Object,
                _mockPendingRepo.Object,
                new Mock<IInteractionRepository>().Object,
                _mockVolumeRepo.Object,
                _mockMapper.Object
            );
        }

        // =========================================================================
        // Testes de Atualização de Metadados (Artigo)
        // =========================================================================

        [Fact]
        public async Task AtualizarMetadadosArtigoAsync_ShouldThrowUnauthorizedException_WhenUserIsNotStaffOrAuthor()
        {
            // Arrange: Configurações específicas para este teste.
            var draftArtigo = new Artigo.Intf.Entities.Artigo { Id = TestArtigoId, Status = StatusArtigo.Rascunho, EditorialId = "editorial_1" };
            // Este usuário 'unauthorized' não está na equipe editorial:
            var editorialRecord = new Editorial { Id = "editorial_1", Team = new EditorialTeam { InitialAuthorId = new List<string> { "user_autor_real_123" } } };
            _mockArtigoRepo.Setup(r => r.GetByIdAsync(TestArtigoId, null)).ReturnsAsync(draftArtigo);
            _mockEditorialRepo.Setup(r => r.GetByIdAsync("editorial_1", null)).ReturnsAsync(editorialRecord);
            var unauthorizedUpdate = new UpdateArtigoMetadataInput { Titulo = "Novo Titulo" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _artigoService.AtualizarMetadadosArtigoAsync(TestArtigoId, unauthorizedUpdate, UnauthorizedUserId, TestCommentary)
            );
        }

        [Fact]
        public async Task AtualizarMetadadosArtigoAsync_ShouldSucceed_WhenUserIsOnEditorialTeam()
        {
            // Arrange
            var draftArtigo = new Artigo.Intf.Entities.Artigo { Id = TestArtigoId, Status = StatusArtigo.Rascunho, EditorialId = "editorial_1", Titulo = "Titulo Antigo", PermitirComentario = true };
            var update = new UpdateArtigoMetadataInput { Titulo = "Titulo Atualizado", Status = StatusArtigo.Arquivado, PermitirComentario = false };
            var autorRecord = new Autor { Id = TestAutorId, UsuarioId = TestAutorUsuarioId };
            var editorialRecord = new Editorial { Id = "editorial_1", Team = new EditorialTeam { InitialAuthorId = new List<string> { TestAutorUsuarioId } } }; // Usuário autorizado é o autor.

            _mockArtigoRepo.Setup(r => r.GetByIdAsync(TestArtigoId, null)).ReturnsAsync(draftArtigo);
            // Staff Mocked to return NULL (não é Staff)
            _mockAutorRepo.Setup(r => r.GetByUsuarioIdAsync(TestAutorUsuarioId, null)).ReturnsAsync(autorRecord);
            _mockEditorialRepo.Setup(r => r.GetByIdAsync("editorial_1", null)).ReturnsAsync(editorialRecord);
            _mockArtigoRepo.Setup(r => r.UpdateAsync(It.IsAny<Artigo.Intf.Entities.Artigo>(), null)).ReturnsAsync(true);

            // Act
            var result = await _artigoService.AtualizarMetadadosArtigoAsync(TestArtigoId, update, TestAutorUsuarioId, TestCommentary);

            // Assert
            Assert.True(result);
            _mockArtigoRepo.Verify(r => r.UpdateAsync(It.Is<Artigo.Intf.Entities.Artigo>(
                a => a.Titulo == "Titulo Atualizado" && a.Status == StatusArtigo.Arquivado && a.PermitirComentario == false
            ), null), Times.Once);
        }

        [Fact]
        public async Task AtualizarMetadadosArtigoAsync_ShouldUpdatePosition_WhenUserIsAdmin()
        {
            // Arrange
            var adminStaff = new Staff { UsuarioId = AdminUserId, Job = FuncaoTrabalho.Administrador, IsActive = true };
            var artigo = new Artigo.Intf.Entities.Artigo { Id = TestArtigoId, EditorialId = TestEditorialId };
            var editorial = new Editorial { Id = TestEditorialId, Position = PosicaoEditorial.Submetido };
            var input = new UpdateArtigoMetadataInput { Posicao = PosicaoEditorial.AguardandoRevisao };

            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, null)).ReturnsAsync(adminStaff);
            _mockArtigoRepo.Setup(r => r.GetByIdAsync(TestArtigoId, null)).ReturnsAsync(artigo);
            _mockEditorialRepo.Setup(r => r.GetByIdAsync(TestEditorialId, null)).ReturnsAsync(editorial);
            _mockEditorialRepo.Setup(r => r.UpdatePositionAsync(TestEditorialId, PosicaoEditorial.AguardandoRevisao, null)).ReturnsAsync(true);

            // Act
            await _artigoService.AtualizarMetadadosArtigoAsync(TestArtigoId, input, AdminUserId, "Mudando Posição");

            // Assert
            _mockEditorialRepo.Verify(r => r.UpdatePositionAsync(TestEditorialId, PosicaoEditorial.AguardandoRevisao, null), Times.Once);
            _mockArtigoRepo.Verify(r => r.UpdateAsync(It.IsAny<Artigo.Intf.Entities.Artigo>(), null), Times.Never);
        }

        // =========================================================================
        // Testes de Pending Request (Criação e Resolução)
        // =========================================================================

        [Fact]
        public async Task AtualizarMetadadosArtigoAsync_ShouldCreatePendingRequest_WhenUserIsEditorBolsista()
        {
            // Arrange: Bolsista (EditorBolsistaUserId) é Staff, mas não Admin/Chefe, então cria Pending.
            var updateInput = new UpdateArtigoMetadataInput { Titulo = "Titulo Pendente", Posicao = PosicaoEditorial.AguardandoRevisao };
            var artigo = new Artigo.Intf.Entities.Artigo { Id = TestArtigoId, EditorialId = TestEditorialId, Status = StatusArtigo.Rascunho };
            _mockArtigoRepo.Setup(r => r.GetByIdAsync(TestArtigoId, null)).ReturnsAsync(artigo);

            // Act
            var result = await _artigoService.AtualizarMetadadosArtigoAsync(TestArtigoId, updateInput, EditorBolsistaUserId, "Comentário de Bolsista");

            // Assert
            Assert.True(result);
            _mockPendingRepo.Verify(r => r.AddAsync(It.Is<Pending>(
                p => p.TargetEntityId == TestArtigoId &&
                     p.CommandType == "UpdateArtigoMetadata" &&
                     p.RequesterUsuarioId == EditorBolsistaUserId &&
                     p.CommandParametersJson.Contains("AguardandoRevisao")
            ), null), Times.Once);
            _mockArtigoRepo.Verify(r => r.UpdateAsync(It.IsAny<Artigo.Intf.Entities.Artigo>(), null), Times.Never);
            _mockEditorialRepo.Verify(r => r.UpdatePositionAsync(It.IsAny<string>(), It.IsAny<PosicaoEditorial>(), null), Times.Never);
        }

        [Fact]
        public async Task AlterarStatusArtigoAsync_ShouldExecuteDirectly_WhenUserIsEditorChefe()
        {
            // Arrange
            _mockArtigoRepo.Setup(r => r.GetByIdAsync(TestArtigoId, null))
                .ReturnsAsync(new Artigo.Intf.Entities.Artigo { Id = TestArtigoId, Status = StatusArtigo.Rascunho });
            _mockArtigoRepo.Setup(r => r.UpdateAsync(It.IsAny<Artigo.Intf.Entities.Artigo>(), null)).ReturnsAsync(true);

            // Act
            var result = await _artigoService.AlterarStatusArtigoAsync(TestArtigoId, StatusArtigo.EmRevisao, EditorChefeUserId, "Comentário de Chefe");

            // Assert
            Assert.True(result);
            _mockPendingRepo.Verify(r => r.AddAsync(It.IsAny<Pending>(), null), Times.Never);
            _mockArtigoRepo.Verify(r => r.UpdateAsync(It.Is<Artigo.Intf.Entities.Artigo>(
                a => a.Id == TestArtigoId && a.Status == StatusArtigo.EmRevisao
            ), null), Times.Once);
        }

        [Fact]
        public async Task ResolverRequisicaoPendenteAsync_ShouldUpdatePosition_WhenCommandIsUpdateArtigoMetadata()
        {
            // Arrange
            var adminStaff = new Staff { UsuarioId = AdminUserId, Job = FuncaoTrabalho.Administrador, IsActive = true };
            var artigo = new Artigo.Intf.Entities.Artigo { Id = TestArtigoId, EditorialId = TestEditorialId };
            var editorial = new Editorial { Id = TestEditorialId, Position = PosicaoEditorial.Submetido };
            var input = new UpdateArtigoMetadataInput { Titulo = "Novo Titulo", Posicao = PosicaoEditorial.AguardandoRevisao };
            var pendingReq = new Pending
            {
                Id = "pending_123",
                CommandType = "UpdateArtigoMetadata",
                TargetEntityId = TestArtigoId,
                CommandParametersJson = JsonSerializer.Serialize(input, _jsonSerializerOptions)
            };

            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, _sessionHandle)).ReturnsAsync(adminStaff);
            _mockPendingRepo.Setup(r => r.GetByIdAsync("pending_123", _sessionHandle)).ReturnsAsync(pendingReq);
            _mockArtigoRepo.Setup(r => r.GetByIdAsync(TestArtigoId, _sessionHandle)).ReturnsAsync(artigo);
            _mockEditorialRepo.Setup(r => r.GetByIdAsync(TestEditorialId, _sessionHandle)).ReturnsAsync(editorial);
            _mockEditorialRepo.Setup(r => r.UpdatePositionAsync(TestEditorialId, PosicaoEditorial.AguardandoRevisao, _sessionHandle)).ReturnsAsync(true);
            _mockArtigoRepo.Setup(r => r.UpdateAsync(It.IsAny<Artigo.Intf.Entities.Artigo>(), _sessionHandle)).ReturnsAsync(true);

            // Act
            var result = await _artigoService.ResolverRequisicaoPendenteAsync("pending_123", true, AdminUserId);

            // Assert
            Assert.True(result);
            _mockEditorialRepo.Verify(r => r.UpdatePositionAsync(TestEditorialId, PosicaoEditorial.AguardandoRevisao, _sessionHandle), Times.Once);
            _mockArtigoRepo.Verify(r => r.UpdateAsync(It.Is<Artigo.Intf.Entities.Artigo>(a => a.Titulo == "Novo Titulo"), _sessionHandle), Times.Once);
        }

        [Fact]
        public async Task ResolverRequisicaoPendenteAsync_ShouldUpdateEditorialTeam_WhenCommandIsValid()
        {
            // Arrange
            var team = new EditorialTeam { EditorIds = new List<string> { "new_editor_id" } };
            var editorial = new Editorial { Id = "editorial_123" };
            var pendingReq = new Pending
            {
                Id = "pending_123",
                CommandType = "UpdateEditorialTeam",
                TargetEntityId = "editorial_123",
                CommandParametersJson = JsonSerializer.Serialize(team, _jsonSerializerOptions)
            };

            _mockPendingRepo.Setup(r => r.GetByIdAsync("pending_123", _sessionHandle)).ReturnsAsync(pendingReq);
            _mockEditorialRepo.Setup(r => r.GetByIdAsync("editorial_123", _sessionHandle)).ReturnsAsync(editorial);
            _mockEditorialRepo.Setup(r => r.UpdateTeamAsync("editorial_123", It.IsAny<EditorialTeam>(), _sessionHandle)).ReturnsAsync(true);
            _mockPendingRepo.Setup(r => r.UpdateAsync(It.IsAny<Pending>(), _sessionHandle)).ReturnsAsync(true);

            // Act
            var result = await _artigoService.ResolverRequisicaoPendenteAsync("pending_123", true, AdminUserId);

            // Assert
            Assert.True(result);
            _mockEditorialRepo.Verify(r => r.UpdateTeamAsync("editorial_123", It.Is<EditorialTeam>(t => t.EditorIds.Contains("new_editor_id")), _sessionHandle), Times.Once);
            _mockPendingRepo.Verify(r => r.UpdateAsync(It.Is<Pending>(p => p.Status == StatusPendente.Aprovado), _sessionHandle), Times.Once);
        }

        [Fact]
        public async Task ResolverRequisicaoPendenteAsync_ShouldUpdateStaff_WhenCommandIsValid()
        {
            // Arrange
            var updateInput = new UpdateStaffInput { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorChefe };
            var pendingReq = new Pending
            {
                Id = "pending_123",
                CommandType = "UpdateStaff",
                TargetEntityId = EditorBolsistaUserId,
                CommandParametersJson = JsonSerializer.Serialize(updateInput, _jsonSerializerOptions)
            };
            var bolsistaStaff = new Staff { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorBolsista, IsActive = true };

            _mockPendingRepo.Setup(r => r.GetByIdAsync("pending_123", _sessionHandle)).ReturnsAsync(pendingReq);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId, _sessionHandle)).ReturnsAsync(bolsistaStaff);
            _mockStaffRepo.Setup(r => r.UpdateAsync(It.IsAny<Staff>(), _sessionHandle)).ReturnsAsync(true);
            _mockPendingRepo.Setup(r => r.UpdateAsync(It.IsAny<Pending>(), _sessionHandle)).ReturnsAsync(true);

            // Act
            var result = await _artigoService.ResolverRequisicaoPendenteAsync("pending_123", true, AdminUserId);

            // Assert
            Assert.True(result);
            _mockStaffRepo.Verify(r => r.UpdateAsync(It.Is<Staff>(
                s => s.UsuarioId == EditorBolsistaUserId && s.Job == FuncaoTrabalho.EditorChefe
            ), _sessionHandle), Times.Once);
            _mockPendingRepo.Verify(r => r.UpdateAsync(It.Is<Pending>(p => p.Status == StatusPendente.Aprovado), _sessionHandle), Times.Once);
        }

        // =========================================================================
        // Testes de Staff (Criação e Atualização)
        // =========================================================================

        [Fact]
        public async Task CriarNovoStaffAsync_ShouldCreatePendingRequest_WhenUserIsEditorBolsista()
        {
            // Arrange
            var novoStaff = new Staff { UsuarioId = NewStaffUserId, Job = FuncaoTrabalho.EditorBolsista };

            // Act
            var result = await _artigoService.CriarNovoStaffAsync(novoStaff, EditorBolsistaUserId, TestCommentary);

            // Assert
            Assert.NotNull(result);
            _mockPendingRepo.Verify(r => r.AddAsync(It.Is<Pending>(
                p => p.TargetEntityId == NewStaffUserId && p.CommandType == "CreateStaff"
            ), null), Times.Once);
            _mockStaffRepo.Verify(r => r.AddAsync(It.IsAny<Staff>(), null), Times.Never);
        }

        [Fact]
        public async Task CriarNovoStaffAsync_ShouldExecuteDirectly_WhenAdmin()
        {
            // Arrange
            var novoStaff = new Staff { UsuarioId = NewStaffUserId, Job = FuncaoTrabalho.EditorBolsista, Nome = "Novo Staff", Url = "url.com" };
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(NewStaffUserId, _sessionHandle)).ReturnsAsync((Staff?)null);

            // Act
            var result = await _artigoService.CriarNovoStaffAsync(novoStaff, AdminUserId, TestCommentary);

            // Assert
            Assert.NotNull(result);
            _mockPendingRepo.Verify(r => r.AddAsync(It.IsAny<Pending>(), null), Times.Never);
            _mockStaffRepo.Verify(r => r.AddAsync(It.Is<Staff>(
                s => s.UsuarioId == NewStaffUserId && s.Nome == "Novo Staff"
            ), _sessionHandle), Times.Once);
        }

        [Fact]
        public async Task CriarNovoStaffAsync_ShouldThrowInvalidOperationException_WhenStaffAlreadyExists_AndUserIsAdmin()
        {
            // Arrange
            var existingStaff = new Staff { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorBolsista };
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId, _sessionHandle)).ReturnsAsync(existingStaff);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _artigoService.CriarNovoStaffAsync(existingStaff, AdminUserId, TestCommentary)
            );
            _mockStaffRepo.Verify(r => r.AddAsync(It.IsAny<Staff>(), _sessionHandle), Times.Never);
        }

        [Fact]
        public async Task AtualizarStaffAsync_ShouldCreatePendingRequest_WhenUserIsEditorBolsista()
        {
            // Arrange
            var updateInput = new UpdateStaffInput { UsuarioId = AdminUserId, Job = FuncaoTrabalho.Aposentado };
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, null))
                .ReturnsAsync(new Staff { UsuarioId = AdminUserId, Job = FuncaoTrabalho.Administrador, IsActive = true });

            // Act
            var result = await _artigoService.AtualizarStaffAsync(updateInput, EditorBolsistaUserId, "Pedido de aposentadoria");

            // Assert
            _mockPendingRepo.Verify(r => r.AddAsync(It.Is<Pending>(
                p => p.TargetEntityId == AdminUserId &&
                     p.CommandType == "UpdateStaff" &&
                     p.RequesterUsuarioId == EditorBolsistaUserId
            ), null), Times.Once);
            _mockStaffRepo.Verify(r => r.UpdateAsync(It.IsAny<Staff>(), null), Times.Never);
            Assert.NotNull(result);
            Assert.Equal(FuncaoTrabalho.Administrador, result.Job);
        }

        [Fact]
        public async Task AtualizarStaffAsync_ShouldExecuteDirectly_WhenUserIsAdmin()
        {
            // Arrange
            var updateInput = new UpdateStaffInput { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorChefe };
            var bolsistaStaff = new Staff { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorBolsista, IsActive = true };

            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId, null))
                .ReturnsAsync(bolsistaStaff);
            _mockStaffRepo.Setup(r => r.UpdateAsync(It.IsAny<Staff>(), null)).ReturnsAsync(true);

            // Act
            var result = await _artigoService.AtualizarStaffAsync(updateInput, AdminUserId, "Promoção por Admin");

            // Assert
            _mockPendingRepo.Verify(r => r.AddAsync(It.IsAny<Pending>(), null), Times.Never);
            _mockStaffRepo.Verify(r => r.UpdateAsync(It.Is<Staff>(
                s => s.UsuarioId == EditorBolsistaUserId &&
                     s.Job == FuncaoTrabalho.EditorChefe
            ), null), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(FuncaoTrabalho.EditorChefe, result.Job);
        }

        [Fact]
        public async Task AtualizarStaffAsync_ShouldThrowKeyNotFound_WhenTargetNotFound()
        {
            // Arrange
            var updateInput = new UpdateStaffInput { UsuarioId = "id_inexistente", Job = FuncaoTrabalho.EditorChefe };
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync("id_inexistente", null)).ReturnsAsync((Staff?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _artigoService.AtualizarStaffAsync(updateInput, AdminUserId, "Teste de falha")
            );
        }

        // =========================================================================
        // Testes de Busca (Pública e Staff)
        // =========================================================================

        [Fact]
        public async Task ObterArtigosCardListPorNomeAutorAsync_ShouldCallBothRepositories()
        {
            // Arrange
            string searchTerm = "autor";
            var autoresRegistrados = new List<Autor> { new Autor { Id = "autor_123" } };
            _mockAutorRepo.Setup(r => r.SearchAutoresByNameAsync(searchTerm, null)).ReturnsAsync(autoresRegistrados);
            var artigosPorId = new List<Artigo.Intf.Entities.Artigo> { new Artigo.Intf.Entities.Artigo { Id = "artigo_1" } };
            _mockArtigoRepo.Setup(r => r.SearchArtigosCardListByAutorIdsAsync(It.Is<IReadOnlyList<string>>(ids => ids.Contains("autor_123")), null)).ReturnsAsync(artigosPorId);
            var artigosPorRef = new List<Artigo.Intf.Entities.Artigo> { new Artigo.Intf.Entities.Artigo { Id = "artigo_2" } };
            _mockArtigoRepo.Setup(r => r.SearchArtigosCardListByAutorReferenceAsync(searchTerm, null)).ReturnsAsync(artigosPorRef);

            // Act
            var result = await _artigoService.ObterArtigosCardListPorNomeAutorAsync(searchTerm, 0, 10);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.Id == "artigo_1");
            Assert.Contains(result, a => a.Id == "artigo_2");
        }

        [Fact]
        public async Task ObterArtigosEditorialPorTipoAsync_ShouldCallRepository_WhenUserIsStaff()
        {
            // Arrange
            _mockArtigoRepo.Setup(r => r.ObterArtigosEditorialPorTipoAsync(TipoArtigo.Blog, 0, 10, null)).ReturnsAsync(new List<Artigo.Intf.Entities.Artigo>());

            // Act
            await _artigoService.ObterArtigosEditorialPorTipoAsync(TipoArtigo.Blog, 0, 10, AdminUserId);

            // Assert
            _mockArtigoRepo.Verify(r => r.ObterArtigosEditorialPorTipoAsync(TipoArtigo.Blog, 0, 10, null), Times.Once);
        }

        [Fact]
        public async Task ObterArtigosEditorialPorTipoAsync_ShouldThrowUnauthorized_WhenUserIsNotStaff()
        {
            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _artigoService.ObterArtigosEditorialPorTipoAsync(TipoArtigo.Blog, 0, 10, UnauthorizedUserId)
            );
        }

        [Fact]
        public async Task SearchArtigosEditorialByTitleAsync_ShouldCallRepository_WhenUserIsStaff()
        {
            // Arrange
            _mockArtigoRepo.Setup(r => r.SearchArtigosEditorialByTitleAsync("teste", 0, 10, null)).ReturnsAsync(new List<Artigo.Intf.Entities.Artigo>());

            // Act
            await _artigoService.SearchArtigosEditorialByTitleAsync("teste", 0, 10, AdminUserId);

            // Assert
            _mockArtigoRepo.Verify(r => r.SearchArtigosEditorialByTitleAsync("teste", 0, 10, null), Times.Once);
        }

        [Fact]
        public async Task SearchArtigosEditorialByAutorIdsAsync_ShouldCallRepository_WhenUserIsStaff()
        {
            // Arrange
            var autorIds = new List<string> { TestAutorId };
            _mockArtigoRepo.Setup(r => r.SearchArtigosEditorialByAutorIdsAsync(autorIds, 0, 10, null)).ReturnsAsync(new List<Artigo.Intf.Entities.Artigo>());

            // Act
            await _artigoService.SearchArtigosEditorialByAutorIdsAsync(autorIds, 0, 10, AdminUserId);

            // Assert
            _mockArtigoRepo.Verify(r => r.SearchArtigosEditorialByAutorIdsAsync(autorIds, 0, 10, null), Times.Once);
        }

        // =========================================================================
        // Testes de Autor
        // =========================================================================

        [Fact]
        public async Task ObterAutorPorIdAsync_ShouldSucceed_WhenUserIsStaff()
        {
            // Act
            var result = await _artigoService.ObterAutorPorIdAsync(TestAutorId, AdminUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestAutorId, result.Id);
            _mockAutorRepo.Verify(r => r.GetByIdAsync(TestAutorId, null), Times.Once);
        }

        [Fact]
        public async Task ObterAutorPorIdAsync_ShouldSucceed_WhenUserIsOwner()
        {
            // Act
            var result = await _artigoService.ObterAutorPorIdAsync(TestAutorId, TestAutorUsuarioId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestAutorUsuarioId, result.UsuarioId);
            _mockAutorRepo.Verify(r => r.GetByIdAsync(TestAutorId, null), Times.Once);
        }

        [Fact]
        public async Task ObterAutorPorIdAsync_ShouldThrowUnauthorized_WhenUserIsNotStaffOrOwner()
        {
            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _artigoService.ObterAutorPorIdAsync(TestAutorId, UnauthorizedUserId)
            );
        }

        // =========================================================================
        // Testes de 'Meus Artigos'
        // =========================================================================

        [Fact]
        public async Task ObterMeusArtigosCardListAsync_ShouldReturnArticles_WhenUserIsAutor()
        {
            // Arrange
            var autor = new Autor { Id = TestAutorId, UsuarioId = TestAutorUsuarioId, ArtigoWorkIds = new List<string> { "art_1", "art_2" } };
            var articles = new List<Artigo.Intf.Entities.Artigo> { new Artigo.Intf.Entities.Artigo { Id = "art_1" } };
            _mockAutorRepo.Setup(r => r.GetByUsuarioIdAsync(TestAutorUsuarioId, null)).ReturnsAsync(autor);
            _mockArtigoRepo.Setup(r => r.ObterArtigosCardListPorAutorIdAsync(TestAutorId, null)).ReturnsAsync(articles);

            // Act
            var result = await _artigoService.ObterMeusArtigosCardListAsync(TestAutorUsuarioId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _mockArtigoRepo.Verify(r => r.ObterArtigosCardListPorAutorIdAsync(TestAutorId, null), Times.Once);
        }

        [Fact]
        public async Task ObterMeusArtigosCardListAsync_ShouldReturnEmptyList_WhenUserIsNotAutor()
        {
            // Arrange
            _mockAutorRepo.Setup(r => r.GetByUsuarioIdAsync(UnauthorizedUserId, null)).ReturnsAsync((Autor?)null);

            // Act
            var result = await _artigoService.ObterMeusArtigosCardListAsync(UnauthorizedUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockArtigoRepo.Verify(r => r.ObterArtigosCardListPorAutorIdAsync(It.IsAny<string>(), null), Times.Never);
        }

        // =========================================================================
        // Testes para VerificarStaffAsync
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
    }
}