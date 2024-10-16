﻿
using Moq;
using QuizDev.Application.UseCases.Responses;
using QuizDev.Core.Entities;
using QuizDev.Core.Enums;
using QuizDev.Core.Repositories;

namespace UnitTests.UseCases.Responses;

public class CreateResponseUseCaseTests
{
    private readonly Mock<IAnswerOptionRepository> _answerOptionRepositoryMock;
    private readonly Mock<IResponseRepository> _responseRepositoryMock;
    private readonly Mock<IMatchRepository> _matchRepositoryMock;
    private readonly CreateResponseUseCase _useCase;

    public CreateResponseUseCaseTests()
    {
        _answerOptionRepositoryMock = new Mock<IAnswerOptionRepository>();
        _responseRepositoryMock = new Mock<IResponseRepository>();
        _matchRepositoryMock = new Mock<IMatchRepository>();
        _useCase = new CreateResponseUseCase(_answerOptionRepositoryMock.Object, _responseRepositoryMock.Object, _matchRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_ValidInput_ReturnsResult()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var question = new Question { Id = Guid.NewGuid() };
        var answerOption = new AnswerOption { Id = Guid.NewGuid(), Question = question, QuestionId = question.Id, IsCorrectOption = true };
        var quiz = new Quiz { ExpiresInSeconds = 120, Questions = [ question, new () ] };
        var match = new QuizDev.Core.Entities.Match { Status = QuizDev.Core.Enums.EMatchStatus.Created, Quiz = quiz, UserId = userId, CreatedAt = DateTime.UtcNow, Responses = [] };

        _answerOptionRepositoryMock.Setup(x => x.GetById(answerOption.Id)).ReturnsAsync(answerOption);
        _matchRepositoryMock.Setup(x => x.GetAsync(match.Id)).ReturnsAsync(match);

        //Act
        var result = await _useCase.Execute(userId, match.Id, answerOption.Id);

        //Assert
        _responseRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Response>()), Times.Once);
        _matchRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<QuizDev.Core.Entities.Match>()), Times.Once);
        Assert.Equal(EMatchStatus.Created, match.Status);
        Assert.Equal(1, match.Score);
        Assert.Single(match.Responses);
    }

    [Fact]
    public async Task Execute_AlreadyExpired_ThrowsInvalidOperationException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var question = new Question { Id = Guid.NewGuid() };
        var answerOption = new AnswerOption { Id = Guid.NewGuid(), Question = question, QuestionId = question.Id, IsCorrectOption = true };
        var quiz = new Quiz { ExpiresInSeconds = 11, Questions = [question, new()], Expires = true };
        var match = new QuizDev.Core.Entities.Match { Status = QuizDev.Core.Enums.EMatchStatus.Created, Quiz = quiz, UserId = userId, CreatedAt = DateTime.UtcNow, Responses = [] };

        _answerOptionRepositoryMock.Setup(x => x.GetById(answerOption.Id)).ReturnsAsync(answerOption);
        _matchRepositoryMock.Setup(x => x.GetAsync(match.Id)).ReturnsAsync(match);

        await Task.Delay(quiz.ExpiresInSeconds * 1000); /* Expires match */

        //Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(userId, match.Id, answerOption.Id));
        _matchRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<QuizDev.Core.Entities.Match>()), Times.Once);
        Assert.Equal(EMatchStatus.Failed, match.Status);
    }

    [Fact]
    public async Task Execute_AlreadyFinished_ThrowsInvalidOperationException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var question = new Question { Id = Guid.NewGuid() };
        var answerOption = new AnswerOption { Id = Guid.NewGuid(), Question = question, QuestionId = question.Id, IsCorrectOption = true };
        var quiz = new Quiz { ExpiresInSeconds = 11, Questions = [question, new()] };
        var match = new QuizDev.Core.Entities.Match { Status = QuizDev.Core.Enums.EMatchStatus.Finished  /* Finished */, Quiz = quiz, UserId = userId, CreatedAt = DateTime.UtcNow, Responses = [] };

        _answerOptionRepositoryMock.Setup(x => x.GetById(answerOption.Id)).ReturnsAsync(answerOption);
        _matchRepositoryMock.Setup(x => x.GetAsync(match.Id)).ReturnsAsync(match);

        //Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(userId, match.Id, answerOption.Id));
        _answerOptionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<AnswerOption>()), Times.Never);
    }


    [Fact]
    public async Task Execute_AlreadyFailed_ThrowsInvalidOperationException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var question = new Question { Id = Guid.NewGuid() };
        var answerOption = new AnswerOption { Id = Guid.NewGuid(), Question = question, QuestionId = question.Id, IsCorrectOption = true };
        var quiz = new Quiz { ExpiresInSeconds = 11, Questions = [question, new()] };
        var match = new QuizDev.Core.Entities.Match { Status = QuizDev.Core.Enums.EMatchStatus.Failed /* Failed */, Quiz = quiz, UserId = userId, CreatedAt = DateTime.UtcNow, Responses = [] };

        _answerOptionRepositoryMock.Setup(x => x.GetById(answerOption.Id)).ReturnsAsync(answerOption);
        _matchRepositoryMock.Setup(x => x.GetAsync(match.Id)).ReturnsAsync(match);

        //Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(userId, match.Id, answerOption.Id));
        _answerOptionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<AnswerOption>()), Times.Never);
    }

    [Fact]
    public async Task Execute_UserDoesNotHaveAccess_ThrowsUnauthorizedException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var question = new Question { Id = Guid.NewGuid() };
        var answerOption = new AnswerOption { Id = Guid.NewGuid(), Question = question, QuestionId = question.Id, IsCorrectOption = true };
        var quiz = new Quiz { ExpiresInSeconds = 11, Questions = [question, new()] };
        var match = new QuizDev.Core.Entities.Match { Status = QuizDev.Core.Enums.EMatchStatus.Created, Quiz = quiz, UserId = Guid.NewGuid() /* random user id*/, CreatedAt = DateTime.UtcNow, Responses = [] };

        _answerOptionRepositoryMock.Setup(x => x.GetById(answerOption.Id)).ReturnsAsync(answerOption);
        _matchRepositoryMock.Setup(x => x.GetAsync(match.Id)).ReturnsAsync(match);

        //Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.Execute(userId, match.Id, answerOption.Id));
        _answerOptionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<AnswerOption>()), Times.Never);
    }


    [Fact]
    public async Task Execute_LastQuestion_MustFinish_ReturnsResult()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var question = new Question { Id = Guid.NewGuid() };
        var answerOption = new AnswerOption { Id = Guid.NewGuid(), Question = question, QuestionId = question.Id, IsCorrectOption = true };
        var quiz = new Quiz { ExpiresInSeconds = 120, Questions = [question] /* Just one question */ };
        var match = new QuizDev.Core.Entities.Match { Status = QuizDev.Core.Enums.EMatchStatus.Created, Quiz = quiz, UserId = userId, CreatedAt = DateTime.UtcNow, Responses = [] };

        _answerOptionRepositoryMock.Setup(x => x.GetById(answerOption.Id)).ReturnsAsync(answerOption);
        _matchRepositoryMock.Setup(x => x.GetAsync(match.Id)).ReturnsAsync(match);

        //Act
        var result = await _useCase.Execute(userId, match.Id, answerOption.Id);

        //Assert
        _responseRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Response>()), Times.Once);
        _matchRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<QuizDev.Core.Entities.Match>()), Times.AtMost(2));
        Assert.Equal(EMatchStatus.Finished, match.Status);
    }
}
