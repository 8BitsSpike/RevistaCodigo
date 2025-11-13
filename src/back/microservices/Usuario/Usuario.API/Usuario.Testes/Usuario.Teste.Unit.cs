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
            var contextMock = new Mock<MongoDbContext>(Mock.Of<Microsoft.Extensions.Options.IOptions<Usuario.DbContext.Persistence.UsuarioDatabaseSettings>>());
            _configurationMock = new Mock<IConfiguration>();
            _userServiceMock = new Mock<UsuarioService>(contextMock.Object, _configurationMock.Object);
            _controller = new UsuarioController(_userServiceMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtActionResult_WhenUserIsValid()
        {
            
            var usuarioDto = new UsuarioDto { Name = "Test User", Email = "test@test.com", Password = "password" };
           
            var createdUsuario = new Usuario.Intf.Models.Usuario { Id = ObjectId.GenerateNewId(), Name = usuarioDto.Name, Email = usuarioDto.Email };

           
            _userServiceMock.Setup(s => s.FindUser(usuarioDto.Email)).ReturnsAsync((Usuario.Intf.Models.Usuario)null);
            _userServiceMock.Setup(s => s.CreateAsync(usuarioDto)).ReturnsAsync(createdUsuario);

            var result = await _controller.Create(usuarioDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(UsuarioController.Get), createdResult.ActionName);
            Assert.Equal(createdUsuario.Id.ToString(), createdResult.RouteValues["id"]);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNoContent_WhenUserExists()
        {
            var userId = ObjectId.GenerateNewId();
            var existingUser = new Usuario.Intf.Models.Usuario { Id = userId, Name = "Old Name", Email = "old@email.com" };
            var updatedUserDto = new UsuarioDto { Name = "New Name", Email = "new@email.com", Password = "newPassword" };

            var updatedUserModel = new Usuario.Intf.Models.Usuario
            {
                Id = userId,
                Name = updatedUserDto.Name,
                Email = updatedUserDto.Email,
                Password = updatedUserDto.Password

            };

            _userServiceMock.Setup(s => s.GetAsync(userId)).ReturnsAsync(existingUser);
            _userServiceMock.Setup(s => s.UpdateAsync(userId, It.IsAny<Usuario.Intf.Models.Usuario>())).Returns(Task.CompletedTask);

            var result = await _controller.Update(userId.ToString(), updatedUserModel);

            Assert.IsType<NoContentResult>(result);
            _userServiceMock.Verify(s => s.GetAsync(userId), Times.Once);
            _userServiceMock.Verify(s => s.UpdateAsync(userId, It.IsAny<Usuario.Intf.Models.Usuario>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenUserExists()
        {
            var userId = ObjectId.GenerateNewId();
            var existingUser = new Usuario.Intf.Models.Usuario { Id = userId, Name = "Test User" };

            _userServiceMock.Setup(s => s.GetAsync(userId)).ReturnsAsync(existingUser);
            _userServiceMock.Setup(s => s.DeleteAsync(userId)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(userId.ToString());

            Assert.IsType<NoContentResult>(result);
            _userServiceMock.Verify(s => s.DeleteAsync(userId), Times.Once);
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