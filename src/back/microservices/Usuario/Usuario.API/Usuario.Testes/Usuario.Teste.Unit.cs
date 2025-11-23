using Usuario.API.Controllers;
using Usuario.Intf.Models;
using Usuario.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Moq;
using Usuario.DbContext.Persistence;

namespace Usuario.Tests
{
    public class UserControllerTests
    {
        private readonly Mock<UsuarioService> _userServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly UsuarioController _controller;

        public UserControllerTests()
        {
            // mock do MongoDbContext
            var contextMock = new Mock<MongoDbContext>(Mock.Of<Microsoft.Extensions.Options.IOptions<UsuarioDatabaseSettings>>());
            _configurationMock = new Mock<IConfiguration>();

            // Mockando o IConfiguration para retornar uma Key fake quando o Service tentar validar tokens
            _configurationMock.Setup(c => c["Key"]).Returns("ThisIsAVeryLongAndSecureKeyForTestingPurposes");

            _userServiceMock = new Mock<UsuarioService>(contextMock.Object, _configurationMock.Object);
            _controller = new UsuarioController(_userServiceMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtActionResult_WhenUserIsValid()
        {
            var usuarioDto = new UsuarioDto { Name = "Test User", Email = "test@test.com", Password = "password", PasswordConfirm = "password" };
            var createdUsuario = new Usuario.Intf.Models.Usuario { Id = ObjectId.GenerateNewId(), Name = usuarioDto.Name, Email = usuarioDto.Email };

            _userServiceMock.Setup(s => s.FindUser(usuarioDto.Email)).ReturnsAsync((Usuario.Intf.Models.Usuario)null!);
            _userServiceMock.Setup(s => s.CreateAsync(usuarioDto)).ReturnsAsync(createdUsuario);

            var result = await _controller.Create(usuarioDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(UsuarioController.Get), createdResult.ActionName);
            Assert.Equal(createdUsuario.Id.ToString(), createdResult.RouteValues!["id"]);
        }

        [Fact]
        public async Task UpdateUser_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId();
            var token = "fake-valid-jwt-token"; // Token string necessário
            var existingUser = new Usuario.Intf.Models.Usuario { Id = userId, Name = "Old Name", Email = "old@email.com" };
            var updatedUserDto = new Usuario.Intf.Models.Usuario
            {
                Id = userId,
                Name = "New Name",
                Email = "new@email.com",
                Password = "newPassword"
            };

            // Configura o Mock para aceitar o ID E o Token no GetAsync
            _userServiceMock.Setup(s => s.GetAsync(userId, token)).ReturnsAsync(existingUser);

            // Configura o Mock para aceitar ID, Objeto E Token no UpdateAsync
            // E retorna um ServiceResult.Success (não Task.CompletedTask)
            _userServiceMock.Setup(s => s.UpdateAsync(userId, It.IsAny<Usuario.Intf.Models.Usuario>(), token))
                            .ReturnsAsync(ServiceResult.Success("Update Realizado com sucesso"));

            // Act
            var result = await _controller.Update(userId.ToString(), updatedUserDto, token);

            // Assert
            // O controller agora retorna Ok (200) com a mensagem, não NoContent (204)
            Assert.IsType<OkObjectResult>(result);

            _userServiceMock.Verify(s => s.GetAsync(userId, token), Times.Once);
            _userServiceMock.Verify(s => s.UpdateAsync(userId, It.IsAny<Usuario.Intf.Models.Usuario>(), token), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId();
            var token = "fake-valid-jwt-token"; // Token string necessário

            // O controller não chama GetAsync no Delete, ele chama DeleteAsync direto (baseado no código enviado)
            // Mas caso chamasse, precisaria mockar o GetAsync aqui também.

            // Mock do DeleteAsync recebendo ID e Token, retornando ServiceResult
            _userServiceMock.Setup(s => s.DeleteAsync(userId, token))
                            .ReturnsAsync(ServiceResult.Success("Delete Realizado com sucesso"));

            // Act
            var result = await _controller.Delete(userId.ToString(), token);

            // Assert
            // O controller agora retorna Ok (200) com a mensagem
            Assert.IsType<OkObjectResult>(result);

            _userServiceMock.Verify(s => s.DeleteAsync(userId, token), Times.Once);
        }

        [Fact]
        public async Task Authenticate_ReturnsOkWithJwt_WhenCredentialsAreValid()
        {
            var model = new UserDto { Email = "test@email.com", Password = "password123" };
            var user = new Usuario.Intf.Models.Usuario { Id = ObjectId.GenerateNewId(), Email = model.Email, Password = BCrypt.Net.BCrypt.HashPassword(model.Password) };
            var fakeJwt = "fake-jwt-token";

            _userServiceMock.Setup(s => s.FindUser(model.Email)).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.GenerateJwtToken(user)).Returns(fakeJwt);

            var result = await _controller.Authenticate(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}