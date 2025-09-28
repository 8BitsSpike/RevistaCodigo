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
            var contextMock = new Mock<MongoDbContext>(Mock.Of<Microsoft.Extensions.Options.IOptions<Usuario.DbContext.Persistence.UsuarioDatabaseSettings>>());
            _configurationMock = new Mock<IConfiguration>();
            _userServiceMock = new Mock<UsuarioService>(contextMock.Object, _configurationMock.Object);
            _controller = new UsuarioController(_userServiceMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtActionResult_WhenUserIsValid()
        {
            var usuarioDto = new UsuarDto { Name = "Test User", Email = "test@test.com", Password = "password" };
            var createdUsuario = new Usuar { Id = ObjectId.GenerateNewId(), Name = usuarioDto.Name, Email = usuarioDto.Email };

            _userServiceMock.Setup(s => s.AuthAsync(usuarioDto.Email)).ReturnsAsync((Usuar)null);
            _userServiceMock.Setup(s => s.CreateAsync(usuarioDto)).ReturnsAsync(createdUsuario);

            var result = await _controller.Create(usuarioDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(UsuarioController.Get), createdResult.ActionName);
            Assert.Equal(createdUsuario.Id, createdResult.RouteValues["id"]);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNoContent_WhenUserExists()
        {
            var userId = ObjectId.GenerateNewId();
            var existingUser = new Usuar { Id = userId, Name = "Old Name", Email = "old@email.com" };
            var updatedUserDto = new UsuarDto { Name = "New Name", Email = "new@email.com", Password = "newPassword" };

            _userServiceMock.Setup(s => s.GetAsync(userId)).ReturnsAsync(existingUser);
            _userServiceMock.Setup(s => s.UpdateAsync(userId, It.IsAny<Usuar>())).Returns(Task.CompletedTask);

            var result = await _controller.Update(userId, updatedUserDto);

            Assert.IsType<NoContentResult>(result);
            _userServiceMock.Verify(s => s.GetAsync(userId), Times.Once);
            _userServiceMock.Verify(s => s.UpdateAsync(userId, It.IsAny<Usuar>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenUserExists()
        {
            var userId = ObjectId.GenerateNewId();
            var existingUser = new Usuar { Id = userId, Name = "Test User" };

            _userServiceMock.Setup(s => s.GetAsync(userId)).ReturnsAsync(existingUser);
            _userServiceMock.Setup(s => s.DeleteAsync(userId)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(userId);

            Assert.IsType<NoContentResult>(result);
            _userServiceMock.Verify(s => s.DeleteAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Authenticate_ReturnsOkWithJwt_WhenCredentialsAreValid()
        {
            var model = new UserDto { Email = "test@email.com", Password = "password123" };
            var user = new Usuar { Id = ObjectId.GenerateNewId(), Email = model.Email, Password = BCrypt.Net.BCrypt.HashPassword(model.Password) };
            var fakeJwt = "fake-jwt-token";

            _userServiceMock.Setup(s => s.AuthAsync(model.Email)).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.GenerateJwtToken(user)).Returns(fakeJwt);

            var result = await _controller.Authenticate(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}
