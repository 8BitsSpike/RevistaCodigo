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

        private const string TestArtigoId = "artigo_100";
        private const string UnauthorizedUserId = "user_999";
        private const string AuthorizedStaffId = "user_101";
        private const string AdminUserId = "user_admin_01";
        private const string EditorChefeUserId = "user_chefe_01";
        private const string EditorBolsistaUserId = "user_bolsista_01";
        private const string NewStaffUserId = "user_novo_104";
        private const string TestAutorId = "autor_local_001";
        private const string TestAutorUsuarioId = "user_autor_001"; // ID externo do TestAutorId
        private const string TestVolumeId = "volume_local_002";
        private const string TestHistoryId = "history_001";
        private const string TestCommentary = "Teste de comentário";
        private const string InactiveStaffId = "user_inactive_02";

        private readonly object _sessionHandle = new Mock<MongoDB.Driver.IClientSessionHandle>().Object;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };


        public ArtigoServiceUnitTests()
        {
            // Inicialização dos Mocks
            _mockArtigoRepo = new Mock<IArtigoRepository>();
            _mockStaffRepo = new Mock<IStaffRepository>();
            _mockEditorialRepo = new Mock<IEditorialRepository>();
            _mockAutorRepo = new Mock<IAutorRepository>();
            _mockVolumeRepo = new Mock<IVolumeRepository>();
            _mockHistoryRepo = new Mock<IArtigoHistoryRepository>();
            _mockPendingRepo = new Mock<IPendingRepository>();
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            // Configurações Comuns de Mocks de Staff para Testes de Permissão
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(NewStaffUserId, It.IsAny<object>())).ReturnsAsync((Staff?)null);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, It.IsAny<object>())).ReturnsAsync(new Staff { UsuarioId = AdminUserId, Job = FuncaoTrabalho.Administrador, IsActive = true });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorChefeUserId, It.IsAny<object>())).ReturnsAsync(new Staff { UsuarioId = EditorChefeUserId, Job = FuncaoTrabalho.EditorChefe, IsActive = true });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId, It.IsAny<object>())).ReturnsAsync(new Staff { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorBolsista, IsActive = true });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(UnauthorizedUserId, It.IsAny<object>())).ReturnsAsync((Staff?)null);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AuthorizedStaffId, It.IsAny<object>())).ReturnsAsync(new Staff { UsuarioId = AuthorizedStaffId, Job = FuncaoTrabalho.EditorChefe, IsActive = true }); // Usado para sucesso
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(TestAutorUsuarioId, It.IsAny<object>())).ReturnsAsync((Staff?)null); // Garante que o autor não é staff
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(InactiveStaffId, It.IsAny<object>())).ReturnsAsync(new Staff { UsuarioId = InactiveStaffId, Job = FuncaoTrabalho.Aposentado, IsActive = false });

            // Configurações Comuns de Busca Pontual
            _mockAutorRepo.Setup(r => r.GetByIdAsync(TestAutorId, It.IsAny<object>())).ReturnsAsync(new Autor { Id = TestAutorId, UsuarioId = TestAutorUsuarioId });
            _mockVolumeRepo.Setup(r => r.GetByIdAsync(TestVolumeId, It.IsAny<object>())).ReturnsAsync(new Volume { Id = TestVolumeId });
            _mockHistoryRepo.Setup(r => r.GetByIdAsync(TestHistoryId, It.IsAny<object>())).ReturnsAsync(new ArtigoHistory { Id = TestHistoryId });

            // Configuração do Unit of Work
            _mockUow.Setup(u => u.GetSessionHandle()).Returns(_sessionHandle);


            // Instancia o ArtigoService injetando os mocks
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

        [Fact]
        public async Task AtualizarMetadadosArtigoAsync_ShouldThrowUnauthorizedException_WhenUserIsNotStaffOrAuthor()
        {
            // Arrange
            var draftArtigo = new Artigo.Intf.Entities.Artigo
            {
                Id = TestArtigoId,
                Status = StatusArtigo.Rascunho,
                EditorialId = "editorial_1"
            };

            var editorialRecord = new Editorial
            {
                Id = "editorial_1",
                Team = new EditorialTeam { InitialAuthorId = new List<string> { "user_autor_real_123" } }
            };

            _mockArtigoRepo.Setup(r => r.GetByIdAsync(TestArtigoId, null)).ReturnsAsync(draftArtigo);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(UnauthorizedUserId, null)).ReturnsAsync((Staff?)null); // Não é staff
            _mockEditorialRepo.Setup(r => r.GetByIdAsync("editorial_1", null)).ReturnsAsync(editorialRecord);

            var unauthorizedArtigoUpdate = new Artigo.Intf.Inputs.UpdateArtigoMetadataInput { Titulo = "Novo Titulo" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _artigoService.AtualizarMetadadosArtigoAsync(TestArtigoId, unauthorizedArtigoUpdate, UnauthorizedUserId, TestCommentary)
            );
        }

        [Fact]
        public async Task AtualizarMetadadosArtigoAsync_ShouldSucceed_WhenUserIsOnEditorialTeam()
        {
            // Arrange
            var draftArtigo = new Artigo.Intf.Entities.Artigo
            {
                Id = TestArtigoId,
                Status = StatusArtigo.Rascunho,
                EditorialId = "editorial_1",
                Titulo = "Titulo Antigo",
                PermitirComentario = true
            };

            var authorizedArtigoUpdate = new Artigo.Intf.Inputs.UpdateArtigoMetadataInput
            {
                Titulo = "Titulo Atualizado",
                Status = StatusArtigo.Arquivado,
                PermitirComentario = false
            };

            var autorRecord = new Autor { Id = "autor_real_123", UsuarioId = AuthorizedStaffId };
            var editorialRecord = new Editorial
            {
                Id = "editorial_1",
                Team = new EditorialTeam { InitialAuthorId = new List<string> { AuthorizedStaffId } } // Espera o UsuarioId
            };

            _mockArtigoRepo.Setup(r => r.GetByIdAsync(TestArtigoId, null)).ReturnsAsync(draftArtigo);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AuthorizedStaffId, null)).ReturnsAsync((Staff?)null); // Não é staff
            _mockAutorRepo.Setup(r => r.GetByUsuarioIdAsync(AuthorizedStaffId, null)).ReturnsAsync(autorRecord); // É o autor da equipe
            _mockEditorialRepo.Setup(r => r.GetByIdAsync("editorial_1", null)).ReturnsAsync(editorialRecord);
            _mockArtigoRepo.Setup(r => r.UpdateAsync(It.IsAny<Artigo.Intf.Entities.Artigo>(), null)).ReturnsAsync(true);

            // Act
            var result = await _artigoService.AtualizarMetadadosArtigoAsync(TestArtigoId, authorizedArtigoUpdate, AuthorizedStaffId, TestCommentary);

            // Assert
            Assert.True(result);

            _mockArtigoRepo.Verify(r => r.UpdateAsync(It.Is<Artigo.Intf.Entities.Artigo>(
                a => a.Titulo == "Titulo Atualizado" &&
                     a.Status == StatusArtigo.Arquivado &&
                     a.PermitirComentario == false
            ), null), Times.Once);
        }

        // =========================================================================
        // Testes de Pending Request Logic
        // =========================================================================

        [Fact]
        public async Task AtualizarMetadadosArtigoAsync_ShouldCreatePendingRequest_WhenUserIsEditorBolsista()
        {
            // Arrange
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId, null))
                .ReturnsAsync(new Staff { Job = FuncaoTrabalho.EditorBolsista });

            var updateInput = new Artigo.Intf.Inputs.UpdateArtigoMetadataInput { Titulo = "Titulo Pendente" };

            // Act
            var result = await _artigoService.AtualizarMetadadosArtigoAsync(TestArtigoId, updateInput, EditorBolsistaUserId, "Comentário de Bolsista");

            // Assert
            Assert.True(result);
            _mockPendingRepo.Verify(r => r.AddAsync(It.Is<Pending>(
                p => p.TargetEntityId == TestArtigoId &&
                     p.CommandType == "UpdateArtigoMetadata" &&
                     p.RequesterUsuarioId == EditorBolsistaUserId
            ), null), Times.Once);

            _mockArtigoRepo.Verify(r => r.UpdateAsync(It.IsAny<Artigo.Intf.Entities.Artigo>(), null), Times.Never);
        }

        [Fact]
        public async Task AlterarStatusArtigoAsync_ShouldExecuteDirectly_WhenUserIsEditorChefe()
        {
            // Arrange
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorChefeUserId, null))
                .ReturnsAsync(new Staff { Job = FuncaoTrabalho.EditorChefe });
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

        // =========================================================================
        // Testes de Staff
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

            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, _sessionHandle))
                .ReturnsAsync(new Staff { Job = FuncaoTrabalho.Administrador, IsActive = true });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(NewStaffUserId, _sessionHandle))
                .ReturnsAsync((Staff?)null);

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

            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, _sessionHandle))
                .ReturnsAsync(new Staff { Job = FuncaoTrabalho.Administrador, IsActive = true });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId, _sessionHandle))
                .ReturnsAsync(existingStaff);


            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _artigoService.CriarNovoStaffAsync(existingStaff, AdminUserId, TestCommentary)
            );

            _mockStaffRepo.Verify(r => r.AddAsync(It.IsAny<Staff>(), _sessionHandle), Times.Never);
        }

        // (NOVO) Testes para AtualizarStaffAsync
        [Fact]
        public async Task AtualizarStaffAsync_ShouldCreatePendingRequest_WhenUserIsEditorBolsista()
        {
            // Arrange
            var updateInput = new UpdateStaffInput { UsuarioId = AdminUserId, Job = FuncaoTrabalho.Aposentado };
            // Configura o Bolsista para ser encontrado
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId, null))
                .ReturnsAsync(new Staff { Job = FuncaoTrabalho.EditorBolsista, IsActive = true });
            // Configura o alvo para ser encontrado (para retornar a entidade original)
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, null))
                .ReturnsAsync(new Staff { UsuarioId = AdminUserId, Job = FuncaoTrabalho.Administrador, IsActive = true });

            // Act
            var result = await _artigoService.AtualizarStaffAsync(updateInput, EditorBolsistaUserId, "Pedido de aposentadoria");

            // Assert
            // 1. Verifica se a pendência foi criada
            _mockPendingRepo.Verify(r => r.AddAsync(It.Is<Pending>(
                p => p.TargetEntityId == AdminUserId &&
                     p.CommandType == "UpdateStaff" &&
                     p.RequesterUsuarioId == EditorBolsistaUserId
            ), null), Times.Once);

            // 2. Verifica se a atualização NÃO foi executada diretamente
            _mockStaffRepo.Verify(r => r.UpdateAsync(It.IsAny<Staff>(), null), Times.Never);

            // 3. Verifica se o resultado é o staff *original*
            Assert.NotNull(result);
            Assert.Equal(FuncaoTrabalho.Administrador, result.Job);
        }

        [Fact]
        public async Task AtualizarStaffAsync_ShouldExecuteDirectly_WhenUserIsAdmin()
        {
            // Arrange
            var updateInput = new UpdateStaffInput { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorChefe };
            var bolsistaStaff = new Staff { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorBolsista, IsActive = true };

            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, null))
                .ReturnsAsync(new Staff { Job = FuncaoTrabalho.Administrador, IsActive = true });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId, null))
                .ReturnsAsync(bolsistaStaff);
            _mockStaffRepo.Setup(r => r.UpdateAsync(It.IsAny<Staff>(), null)).ReturnsAsync(true);

            // Act
            var result = await _artigoService.AtualizarStaffAsync(updateInput, AdminUserId, "Promoção por Admin");

            // Assert
            // 1. Verifica se NENHUMA pendência foi criada
            _mockPendingRepo.Verify(r => r.AddAsync(It.IsAny<Pending>(), null), Times.Never);

            // 2. Verifica se a atualização FOI executada
            _mockStaffRepo.Verify(r => r.UpdateAsync(It.Is<Staff>(
                s => s.UsuarioId == EditorBolsistaUserId &&
                     s.Job == FuncaoTrabalho.EditorChefe
            ), null), Times.Once);

            // 3. Verifica se o resultado é o staff *atualizado*
            Assert.NotNull(result);
            Assert.Equal(FuncaoTrabalho.EditorChefe, result.Job);
        }

        [Fact]
        public async Task AtualizarStaffAsync_ShouldThrowKeyNotFound_WhenTargetNotFound()
        {
            // Arrange
            var updateInput = new UpdateStaffInput { UsuarioId = "id_inexistente", Job = FuncaoTrabalho.EditorChefe };

            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, null))
                .ReturnsAsync(new Staff { Job = FuncaoTrabalho.Administrador, IsActive = true });
            // Configura a busca do alvo para retornar nulo
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync("id_inexistente", null))
                .ReturnsAsync((Staff?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _artigoService.AtualizarStaffAsync(updateInput, AdminUserId, "Teste de falha")
            );
        }

        // =========================================================================
        // Testes de Formatos de Query
        // =========================================================================

        [Fact]
        public async Task ObterArtigosCardListAsync_ShouldCallRepositoryAndReturnEntities()
        {
            // Arrange
            var entities = new List<Artigo.Intf.Entities.Artigo> { new Artigo.Intf.Entities.Artigo { Id = TestArtigoId } };

            _mockArtigoRepo.Setup(r => r.ObterArtigosCardListAsync(0, 10, null)).ReturnsAsync(entities);

            // Act
            var result = await _artigoService.ObterArtigosCardListAsync(0, 10);

            // Assert
            Assert.Equal(entities, result);
            _mockArtigoRepo.Verify(r => r.ObterArtigosCardListAsync(0, 10, null), Times.Once);
            _mockMapper.Verify(m => m.Map<IReadOnlyList<Artigo.Server.DTOs.ArtigoCardListDTO>>(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task ObterAutorCardAsync_ShouldCallRepositoryAndReturnEntity()
        {
            // Arrange
            var entity = new Autor { Id = TestAutorId, UsuarioId = "user_autor" };

            _mockAutorRepo.Setup(r => r.GetByIdAsync(TestAutorId, null)).ReturnsAsync(entity);

            // Act
            var result = await _artigoService.ObterAutorCardAsync(TestAutorId);

            // Assert
            Assert.Equal(entity, result);
            _mockAutorRepo.Verify(r => r.GetByIdAsync(TestAutorId, null), Times.Once);
            _mockMapper.Verify(m => m.Map<Artigo.Server.DTOs.AutorCardDTO>(It.IsAny<object>()), Times.Never);
        }

        // =========================================================================
        // Testes de StaffComentario
        // =========================================================================

        [Fact]
        public async Task AddStaffComentarioAsync_ShouldAddCommentToHistory()
        {
            // Arrange
            var history = new ArtigoHistory { Id = TestHistoryId, StaffComentarios = new List<StaffComentario>(), ArtigoId = TestArtigoId };
            _mockHistoryRepo.Setup(r => r.GetByIdAsync(TestHistoryId, null)).ReturnsAsync(history);
            _mockHistoryRepo.Setup(r => r.UpdateAsync(It.IsAny<ArtigoHistory>(), null)).ReturnsAsync(true);

            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, null)).ReturnsAsync(new Staff { Job = FuncaoTrabalho.Administrador, IsActive = true });


            // Act
            var result = await _artigoService.AddStaffComentarioAsync(TestHistoryId, AdminUserId, "Novo comentário", null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.StaffComentarios);
            Assert.Equal("Novo comentário", result.StaffComentarios[0].Comment);
            Assert.Equal(AdminUserId, result.StaffComentarios[0].UsuarioId);
            _mockHistoryRepo.Verify(r => r.UpdateAsync(history, null), Times.Once);
        }

        [Fact]
        public async Task AtualizarEquipeEditorialAsync_ShouldCreatePendingRequest_WhenUserIsEditorBolsista()
        {
            // Arrange
            var newTeam = new EditorialTeam { EditorId = "new_editor_id" };
            _mockEditorialRepo.Setup(r => r.GetByArtigoIdAsync(TestArtigoId, null)).ReturnsAsync(new Editorial { Id = "editorial_123" });

            // Act
            await _artigoService.AtualizarEquipeEditorialAsync(TestArtigoId, newTeam, EditorBolsistaUserId, "Teste Bolsista Team");

            // Assert
            _mockPendingRepo.Verify(r => r.AddAsync(It.Is<Pending>(
                p => p.TargetType == TipoEntidadeAlvo.Editorial &&
                     p.CommandType == "UpdateEditorialTeam" &&
                     p.RequesterUsuarioId == EditorBolsistaUserId
            ), null), Times.Once);
            _mockEditorialRepo.Verify(r => r.UpdateTeamAsync(It.IsAny<string>(), It.IsAny<EditorialTeam>(), null), Times.Never);
        }

        [Fact]
        public async Task AtualizarEquipeEditorialAsync_ShouldExecuteDirectly_WhenAdmin()
        {
            // Arrange
            var newTeam = new EditorialTeam { EditorId = "new_editor_id" };
            var editorial = new Editorial { Id = "editorial_123", ArtigoId = TestArtigoId };
            _mockEditorialRepo.Setup(r => r.GetByArtigoIdAsync(TestArtigoId, null)).ReturnsAsync(editorial);
            _mockEditorialRepo.Setup(r => r.UpdateTeamAsync("editorial_123", newTeam, null)).ReturnsAsync(true);

            // Act
            var result = await _artigoService.AtualizarEquipeEditorialAsync(TestArtigoId, newTeam, AdminUserId, "Teste Admin Team");

            // Assert
            Assert.Equal(newTeam, result.Team);
            _mockPendingRepo.Verify(r => r.AddAsync(It.IsAny<Pending>(), null), Times.Never);
            _mockEditorialRepo.Verify(r => r.UpdateTeamAsync("editorial_123", newTeam, null), Times.Once);
        }

        // (MODIFICADO) Teste de ResolverRequisicaoPendenteAsync
        [Fact]
        public async Task ResolverRequisicaoPendenteAsync_ShouldUpdateStaff_WhenCommandIsValid()
        {
            // Arrange
            var updateInput = new UpdateStaffInput { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorChefe };
            var pendingReq = new Pending
            {
                Id = "pending_123",
                CommandType = "UpdateStaff",
                TargetEntityId = EditorBolsistaUserId, // O alvo é o UsuarioId
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

        [Fact]
        public async Task ResolverRequisicaoPendenteAsync_ShouldUpdateEditorialTeam_WhenCommandIsValid()
        {
            // Arrange
            var team = new EditorialTeam { EditorId = "new_editor_id" };
            var editorial = new Editorial { Id = "editorial_123" };
            var pendingReq = new Pending
            {
                Id = "pending_123",
                CommandType = "UpdateEditorialTeam",
                TargetEntityId = "editorial_123", // O alvo é o Editorial
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
            _mockEditorialRepo.Verify(r => r.UpdateTeamAsync("editorial_123", It.Is<EditorialTeam>(t => t.EditorId == "new_editor_id"), _sessionHandle), Times.Once);
            _mockPendingRepo.Verify(r => r.UpdateAsync(It.Is<Pending>(p => p.Status == StatusPendente.Aprovado), _sessionHandle), Times.Once);
        }

        // =========================================================================
        // Testes de Busca
        // =========================================================================

        [Fact]
        public async Task ObterArtigosCardListPorTituloAsync_ShouldCallRepository()
        {
            // Arrange
            string searchTerm = "teste";
            int pagina = 0;
            int tamanho = 10;
            var expectedList = new List<Artigo.Intf.Entities.Artigo> { new Artigo.Intf.Entities.Artigo { Id = "artigo_1" } };
            _mockArtigoRepo.Setup(r => r.SearchArtigosCardListByTitleAsync(searchTerm, pagina, tamanho, null)).ReturnsAsync(expectedList);

            // Act
            var result = await _artigoService.ObterArtigosCardListPorTituloAsync(searchTerm, pagina, tamanho);

            // Assert
            Assert.Equal(expectedList, result);
            _mockArtigoRepo.Verify(r => r.SearchArtigosCardListByTitleAsync(searchTerm, pagina, tamanho, null), Times.Once);
        }

        [Fact]
        public async Task ObterArtigosCardListPorNomeAutorAsync_ShouldCallBothRepositories()
        {
            // Arrange
            string searchTerm = "autor";
            int pagina = 0;
            int tamanho = 10;

            // Simula autor registrado
            var autoresRegistrados = new List<Autor> { new Autor { Id = "autor_123" } };
            _mockAutorRepo.Setup(r => r.SearchAutoresByNameAsync(searchTerm, null)).ReturnsAsync(autoresRegistrados);

            // Simula artigos encontrados por ID
            var artigosPorId = new List<Artigo.Intf.Entities.Artigo> { new Artigo.Intf.Entities.Artigo { Id = "artigo_1" } };
            _mockArtigoRepo.Setup(r => r.SearchArtigosCardListByAutorIdsAsync(It.Is<IReadOnlyList<string>>(ids => ids.Contains("autor_123")), null)).ReturnsAsync(artigosPorId);

            // Simula artigos encontrados por referência
            var artigosPorRef = new List<Artigo.Intf.Entities.Artigo> { new Artigo.Intf.Entities.Artigo { Id = "artigo_2" } };
            _mockArtigoRepo.Setup(r => r.SearchArtigosCardListByAutorReferenceAsync(searchTerm, null)).ReturnsAsync(artigosPorRef);

            // Act
            var result = await _artigoService.ObterArtigosCardListPorNomeAutorAsync(searchTerm, pagina, tamanho);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.Id == "artigo_1");
            Assert.Contains(result, a => a.Id == "artigo_2");
            _mockAutorRepo.Verify(r => r.SearchAutoresByNameAsync(searchTerm, null), Times.Once);
            _mockArtigoRepo.Verify(r => r.SearchArtigosCardListByAutorIdsAsync(It.IsAny<IReadOnlyList<string>>(), null), Times.Once);
            _mockArtigoRepo.Verify(r => r.SearchArtigosCardListByAutorReferenceAsync(searchTerm, null), Times.Once);
        }

        [Fact]
        public async Task ObterArtigosCardListPorNomeAutorAsync_ShouldCombineAndPaginateResults()
        {
            // Arrange
            string searchTerm = "autor";
            var dataAntiga = DateTime.UtcNow.AddDays(-1);
            var dataNova = DateTime.UtcNow;

            // Artigo duplicado (mesmo ID, será encontrado em ambas as buscas)
            var artigo1 = new Artigo.Intf.Entities.Artigo { Id = "artigo_1", Titulo = "Duplicado", DataCriacao = dataAntiga };
            // Artigo único por ID
            var artigo2 = new Artigo.Intf.Entities.Artigo { Id = "artigo_2", Titulo = "Unico ID", DataCriacao = dataNova };
            // Artigo único por Referência
            var artigo3 = new Artigo.Intf.Entities.Artigo { Id = "artigo_3", Titulo = "Unico Ref", DataCriacao = dataAntiga };

            _mockAutorRepo.Setup(r => r.SearchAutoresByNameAsync(searchTerm, null)).ReturnsAsync(new List<Autor> { new Autor { Id = "autor_123" } });
            _mockArtigoRepo.Setup(r => r.SearchArtigosCardListByAutorIdsAsync(It.IsAny<IReadOnlyList<string>>(), null)).ReturnsAsync(new List<Artigo.Intf.Entities.Artigo> { artigo1, artigo2 });
            _mockArtigoRepo.Setup(r => r.SearchArtigosCardListByAutorReferenceAsync(searchTerm, null)).ReturnsAsync(new List<Artigo.Intf.Entities.Artigo> { artigo3, artigo1 }); // artigo1 está duplicado

            // Act
            // Busca Página 0, Tamanho 2
            var resultPagina1 = await _artigoService.ObterArtigosCardListPorNomeAutorAsync(searchTerm, 0, 2);

            // Busca Página 1, Tamanho 2
            var resultPagina2 = await _artigoService.ObterArtigosCardListPorNomeAutorAsync(searchTerm, 1, 2);

            // Assert
            // Deve haver 3 resultados únicos no total (artigo2, artigo3, artigo1 - ordenados por data)
            Assert.Equal(2, resultPagina1.Count);
            Assert.Equal("Unico ID", resultPagina1[0].Titulo); // artigo2 (DataNova)
            Assert.Equal("Duplicado", resultPagina1[1].Titulo); // artigo1 (DataAntiga) - A ordem entre 1 e 3 pode variar

            Assert.Single(resultPagina2);
            Assert.Contains(resultPagina2, a => a.Id == "artigo_3" || a.Id == "artigo_1"); // O resultado restante
        }

        // =========================================================================
        // (NOVOS TESTES) Testes para ObterAutorPorIdAsync
        // =========================================================================

        [Fact]
        public async Task ObterAutorPorIdAsync_ShouldSucceed_WhenUserIsStaff()
        {
            // Arrange
            // TestAutorId está configurado para retornar um Autor com UsuarioId = TestAutorUsuarioId
            // AdminUserId está configurado para retornar um Staff Admin

            // Act
            var result = await _artigoService.ObterAutorPorIdAsync(TestAutorId, AdminUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestAutorId, result.Id);
            _mockStaffRepo.Verify(r => r.GetByUsuarioIdAsync(AdminUserId, null), Times.Once);
            _mockAutorRepo.Verify(r => r.GetByIdAsync(TestAutorId, null), Times.Once);
        }

        [Fact]
        public async Task ObterAutorPorIdAsync_ShouldSucceed_WhenUserIsOwner()
        {
            // Arrange
            // TestAutorId está configurado para retornar um Autor com UsuarioId = TestAutorUsuarioId
            // TestAutorUsuarioId está configurado para NÃO retornar um Staff

            // Act
            var result = await _artigoService.ObterAutorPorIdAsync(TestAutorId, TestAutorUsuarioId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestAutorId, result.Id);
            Assert.Equal(TestAutorUsuarioId, result.UsuarioId);
            _mockStaffRepo.Verify(r => r.GetByUsuarioIdAsync(TestAutorUsuarioId, null), Times.Once);
            _mockAutorRepo.Verify(r => r.GetByIdAsync(TestAutorId, null), Times.Once);
        }

        [Fact]
        public async Task ObterAutorPorIdAsync_ShouldThrowUnauthorized_WhenUserIsNotStaffOrOwner()
        {
            // Arrange
            // TestAutorId está configurado para retornar um Autor com UsuarioId = TestAutorUsuarioId
            // UnauthorizedUserId está configurado para NÃO retornar um Staff

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _artigoService.ObterAutorPorIdAsync(TestAutorId, UnauthorizedUserId)
            );

            // Verificações
            _mockStaffRepo.Verify(r => r.GetByUsuarioIdAsync(UnauthorizedUserId, null), Times.Once);
            _mockAutorRepo.Verify(r => r.GetByIdAsync(TestAutorId, null), Times.Once); // Busca o autor para checar o dono
        }

        // =========================================================================
        // (NOVOS TESTES) Testes para ObterMeusArtigosCardListAsync
        // =========================================================================

        [Fact]
        public async Task ObterMeusArtigosCardListAsync_ShouldReturnArticles_WhenUserIsAutor()
        {
            // Arrange
            var autor = new Autor { Id = TestAutorId, UsuarioId = TestAutorUsuarioId, ArtigoWorkIds = new List<string> { "art_1", "art_2" } };
            var articles = new List<Artigo.Intf.Entities.Artigo>
            {
                new Artigo.Intf.Entities.Artigo { Id = "art_1", Status = StatusArtigo.Rascunho },
                new Artigo.Intf.Entities.Artigo { Id = "art_2", Status = StatusArtigo.Publicado }
            };

            _mockAutorRepo.Setup(r => r.GetByUsuarioIdAsync(TestAutorUsuarioId, null)).ReturnsAsync(autor);
            _mockArtigoRepo.Setup(r => r.ObterArtigosCardListPorAutorIdAsync(TestAutorId, null)).ReturnsAsync(articles);

            // Act
            var result = await _artigoService.ObterMeusArtigosCardListAsync(TestAutorUsuarioId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _mockAutorRepo.Verify(r => r.GetByUsuarioIdAsync(TestAutorUsuarioId, null), Times.Once);
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
            _mockAutorRepo.Verify(r => r.GetByUsuarioIdAsync(UnauthorizedUserId, null), Times.Once);
            // NÃO DEVE chamar o repositório de artigo
            _mockArtigoRepo.Verify(r => r.ObterArtigosCardListPorAutorIdAsync(It.IsAny<string>(), null), Times.Never);
        }

        // =========================================================================
        // (NOVOS TESTES) Testes para VerificarStaffAsync
        // =========================================================================

        [Fact]
        public async Task VerificarStaffAsync_ShouldReturnTrue_WhenUserIsActiveStaff()
        {
            // Arrange (AdminUserId já está configurado como Staff ativo)

            // Act
            var result = await _artigoService.VerificarStaffAsync(AdminUserId);

            // Assert
            Assert.True(result);
            _mockStaffRepo.Verify(r => r.GetByUsuarioIdAsync(AdminUserId, null), Times.Once);
        }

        [Fact]
        public async Task VerificarStaffAsync_ShouldReturnFalse_WhenUserIsNotStaff()
        {
            // Arrange (UnauthorizedUserId já está configurado para retornar null)

            // Act
            var result = await _artigoService.VerificarStaffAsync(UnauthorizedUserId);

            // Assert
            Assert.False(result);
            _mockStaffRepo.Verify(r => r.GetByUsuarioIdAsync(UnauthorizedUserId, null), Times.Once);
        }

        [Fact]
        public async Task VerificarStaffAsync_ShouldReturnFalse_WhenUserIsInactiveStaff()
        {
            // Arrange (InactiveStaffId está configurado como IsActive = false)

            // Act
            var result = await _artigoService.VerificarStaffAsync(InactiveStaffId);

            // Assert
            Assert.False(result);
            _mockStaffRepo.Verify(r => r.GetByUsuarioIdAsync(InactiveStaffId, null), Times.Once);
        }
    }
}