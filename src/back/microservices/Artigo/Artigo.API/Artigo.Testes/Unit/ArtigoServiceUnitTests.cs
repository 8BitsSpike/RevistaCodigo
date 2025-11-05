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
using Artigo.Intf.Inputs; // *** ADICIONADO ***

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
        private const string TestVolumeId = "volume_local_002";
        private const string TestHistoryId = "history_001";
        private const string TestCommentary = "Teste de comentário";


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
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(NewStaffUserId, null)).ReturnsAsync((Staff?)null);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, null)).ReturnsAsync(new Staff { UsuarioId = AdminUserId, Job = FuncaoTrabalho.Administrador });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorChefeUserId, null)).ReturnsAsync(new Staff { UsuarioId = EditorChefeUserId, Job = FuncaoTrabalho.EditorChefe });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId, null)).ReturnsAsync(new Staff { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorBolsista });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(UnauthorizedUserId, null)).ReturnsAsync((Staff?)null);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AuthorizedStaffId, null)).ReturnsAsync(new Staff { UsuarioId = AuthorizedStaffId, Job = FuncaoTrabalho.EditorChefe }); // Usado para sucesso

            // Configurações Comuns de Busca Pontual
            _mockAutorRepo.Setup(r => r.GetByIdAsync(TestAutorId, null)).ReturnsAsync(new Autor { Id = TestAutorId, UsuarioId = "user_autor" });
            _mockVolumeRepo.Setup(r => r.GetByIdAsync(TestVolumeId, null)).ReturnsAsync(new Volume { Id = TestVolumeId });
            _mockHistoryRepo.Setup(r => r.GetByIdAsync(TestHistoryId, null)).ReturnsAsync(new ArtigoHistory { Id = TestHistoryId });

            // Configuração do Unit of Work
            _mockUow.Setup(u => u.GetSessionHandle()).Returns(new Mock<MongoDB.Driver.IClientSessionHandle>().Object);


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
                Team = new EditorialTeam { InitialAuthorId = new List<string> { "autor_real_123" } }
            };

            _mockArtigoRepo.Setup(r => r.GetByIdAsync(TestArtigoId, null)).ReturnsAsync(draftArtigo);
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(UnauthorizedUserId, null)).ReturnsAsync((Staff?)null); // Não é staff
            _mockAutorRepo.Setup(r => r.GetByUsuarioIdAsync(UnauthorizedUserId, null)).ReturnsAsync(new Autor { Id = "autor_fake_456" }); // É um autor, mas não o da equipe
            _mockEditorialRepo.Setup(r => r.GetByIdAsync("editorial_1", null)).ReturnsAsync(editorialRecord);

            // *** CORREÇÃO: Alterado de Artigo para UpdateArtigoMetadataInput ***
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
                Titulo = "Titulo Antigo"
            };

            // *** CORREÇÃO: Alterado de Artigo para UpdateArtigoMetadataInput ***
            var authorizedArtigoUpdate = new Artigo.Intf.Inputs.UpdateArtigoMetadataInput
            {
                Titulo = "Titulo Atualizado"
            };

            var autorRecord = new Autor { Id = "autor_real_123", UsuarioId = AuthorizedStaffId };

            var editorialRecord = new Editorial
            {
                Id = "editorial_1",
                Team = new EditorialTeam { InitialAuthorId = new List<string> { "autor_real_123" } }
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
            // Verifica se o UpdateAsync foi chamado com a entidade 'draftArtigo'
            // que foi modificada internamente pelo serviço (ApplyMetadataUpdates)
            _mockArtigoRepo.Verify(r => r.UpdateAsync(It.Is<Artigo.Intf.Entities.Artigo>(
                a => a.Titulo == "Titulo Atualizado"
            ), null), Times.Once);
        }

        // =========================================================================
        // NOVOS TESTES (Pending Request Logic)
        // =========================================================================

        [Fact]
        public async Task AtualizarMetadadosArtigoAsync_ShouldCreatePendingRequest_WhenUserIsEditorBolsista()
        {
            // Arrange
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId, null))
                .ReturnsAsync(new Staff { Job = FuncaoTrabalho.EditorBolsista });

            // *** CORREÇÃO: Alterado de Artigo para UpdateArtigoMetadataInput ***
            var updateInput = new Artigo.Intf.Inputs.UpdateArtigoMetadataInput { Titulo = "Titulo Pendente" };

            // Act
            var result = await _artigoService.AtualizarMetadadosArtigoAsync(TestArtigoId, updateInput, EditorBolsistaUserId, "Comentário de Bolsista");

            // Assert
            Assert.True(result);
            // Verifica se o repositório de Pending foi chamado
            _mockPendingRepo.Verify(r => r.AddAsync(It.Is<Pending>(
                p => p.TargetEntityId == TestArtigoId &&
                     p.CommandType == "UpdateArtigoMetadata" &&
                     p.RequesterUsuarioId == EditorBolsistaUserId
            ), null), Times.Once);

            // Verifica que o repositório de Artigo NÃO foi chamado
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
            // Verifica que o repositório de Pending NÃO foi chamado
            _mockPendingRepo.Verify(r => r.AddAsync(It.IsAny<Pending>(), null), Times.Never);
            // Verifica que o repositório de Artigo FOI chamado
            _mockArtigoRepo.Verify(r => r.UpdateAsync(It.Is<Artigo.Intf.Entities.Artigo>(
                a => a.Id == TestArtigoId && a.Status == StatusArtigo.EmRevisao
            ), null), Times.Once);
        }


        // =========================================================================
        // TESTES DE STAFF MANAGEMENT
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

            // *** CORREÇÃO: Definir a variável de sessão e atualizar mocks ***
            var sessionHandle = _mockUow.Object.GetSessionHandle();

            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, null)) // Chamada inicial (fora da tx)
                .ReturnsAsync(new Staff { Job = FuncaoTrabalho.Administrador });

            // Mocks para chamadas DENTRO da transação
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, sessionHandle))
                .ReturnsAsync(new Staff { Job = FuncaoTrabalho.Administrador });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(NewStaffUserId, sessionHandle))
                .ReturnsAsync((Staff?)null);

            // Act
            var result = await _artigoService.CriarNovoStaffAsync(novoStaff, AdminUserId, TestCommentary);

            // Assert
            Assert.NotNull(result);
            _mockPendingRepo.Verify(r => r.AddAsync(It.IsAny<Pending>(), null), Times.Never);

            // *** CORREÇÃO: Verificar a chamada com o sessionHandle ***
            _mockStaffRepo.Verify(r => r.AddAsync(It.Is<Staff>(
                s => s.UsuarioId == NewStaffUserId && s.Nome == "Novo Staff"
            ), sessionHandle), Times.Once);
        }

        [Fact]
        public async Task CriarNovoStaffAsync_ShouldThrowInvalidOperationException_WhenStaffAlreadyExists_AndUserIsAdmin()
        {
            // Arrange
            var existingStaff = new Staff { UsuarioId = EditorBolsistaUserId, Job = FuncaoTrabalho.EditorBolsista };

            // *** CORREÇÃO: Definir a variável de sessão e atualizar mocks ***
            var sessionHandle = _mockUow.Object.GetSessionHandle();

            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, null)) // Chamada inicial
                .ReturnsAsync(new Staff { Job = FuncaoTrabalho.Administrador });

            // Mocks para chamadas DENTRO da transação
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(AdminUserId, sessionHandle))
                .ReturnsAsync(new Staff { Job = FuncaoTrabalho.Administrador });
            _mockStaffRepo.Setup(r => r.GetByUsuarioIdAsync(EditorBolsistaUserId, sessionHandle))
                .ReturnsAsync(existingStaff);


            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _artigoService.CriarNovoStaffAsync(existingStaff, AdminUserId, TestCommentary)
            );

            // *** CORREÇÃO: Verificar que AddAsync nunca foi chamado COM SESSÃO ***
            _mockStaffRepo.Verify(r => r.AddAsync(It.IsAny<Staff>(), sessionHandle), Times.Never);
        }

        // =========================================================================
        // NOVOS TESTES (Formatos de Query)
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

        // *** TESTE ATUALIZADO (PRIORIDADE 3) ***
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
        // NOVOS TESTES (StaffComentario)
        // =========================================================================

        [Fact]
        public async Task AddStaffComentarioAsync_ShouldAddCommentToHistory()
        {
            // Arrange
            var history = new ArtigoHistory { Id = TestHistoryId, StaffComentarios = new List<StaffComentario>() };
            _mockHistoryRepo.Setup(r => r.GetByIdAsync(TestHistoryId, null)).ReturnsAsync(history);
            _mockHistoryRepo.Setup(r => r.UpdateAsync(It.IsAny<ArtigoHistory>(), null)).ReturnsAsync(true);

            // Act
            var result = await _artigoService.AddStaffComentarioAsync(TestHistoryId, AdminUserId, "Novo comentário", null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.StaffComentarios);
            Assert.Equal("Novo comentário", result.StaffComentarios[0].Comment);
            Assert.Equal(AdminUserId, result.StaffComentarios[0].UsuarioId);
            _mockHistoryRepo.Verify(r => r.UpdateAsync(history, null), Times.Once);
        }
    }
}