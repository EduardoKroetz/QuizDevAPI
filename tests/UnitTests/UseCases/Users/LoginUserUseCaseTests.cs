﻿
using Moq;
using Newtonsoft.Json;
using QuizDev.Application.Services.Interfaces;
using QuizDev.Application.UseCases.Users;
using QuizDev.Core.DTOs.Users;
using QuizDev.Core.Entities;
using QuizDev.Core.Repositories;

namespace UnitTests.UseCases.Users;

public class LoginUserUseCaseTests
{
    private readonly LoginUserUseCase _loginUserUseCase;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    public LoginUserUseCaseTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loginUserUseCase = new LoginUserUseCase(_userRepositoryMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task Execute_LoginUserWithValidCredentials_ReturnsToken()
    {
        //Arrange
        var loginDto = new LoginUserDto { Email = "test@gmail.com", Password = "password" };
        var user = new User { Id = Guid.NewGuid(), PasswordHash = "password_hash", };
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _authServiceMock.Setup(x => x.VerifyPassword(loginDto.Password, user.PasswordHash)).Returns(true);
        _authServiceMock.Setup(x => x.GenerateJwtToken(user)).Returns("token");

        //Act
        var result = await _loginUserUseCase.Execute(loginDto);

        //Assert
        var data = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(result.Data));
        Assert.Equal("token", (string)data.Token);
    }

    [Fact]
    public async Task Execute_LoginUserWithInvalidEmail_ThrowsArgumentException()
    {
        //Arrange
        var loginDto = new LoginUserDto { Email = "test@gmail.com", Password = "password" };
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginDto.Email)).ReturnsAsync((User)null);

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _loginUserUseCase.Execute(loginDto));
    }

    [Fact]
    public async Task Execute_LoginUserWithInvalidPassword_ThrowsArgumentException()
    {
        //Arrange
        var loginDto = new LoginUserDto { Email = "test@gmail.com", Password = "password" };
        var user = new User { Id = Guid.NewGuid(), PasswordHash = "password_hash", };
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _authServiceMock.Setup(x => x.VerifyPassword(loginDto.Password, user.PasswordHash)).Returns(false);

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _loginUserUseCase.Execute(loginDto));
    }
}
