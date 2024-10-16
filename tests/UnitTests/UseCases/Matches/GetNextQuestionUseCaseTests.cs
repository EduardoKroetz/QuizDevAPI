﻿
using Moq;
using Newtonsoft.Json;
using QuizDev.Application.UseCases.Matches;
using QuizDev.Core.Entities;
using QuizDev.Core.Repositories;

namespace UnitTests.UseCases.Matches;

public class GetNextQuestionUseCaseTests
{
    private readonly Mock<IMatchRepository> _matchRepositoryMock;
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly GetNextQuestionUseCase _useCase;

    public GetNextQuestionUseCaseTests()
    {
        _matchRepositoryMock = new Mock<IMatchRepository>();
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _useCase = new GetNextQuestionUseCase(_matchRepositoryMock.Object, _questionRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_ValidInput_ReturnsResult()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var nextQuestion = new Question { Id = Guid.NewGuid(), Order = 0, Text = "", Options = [],  QuizId = Guid.NewGuid() };
        var quiz = new Quiz { Questions = new List<Question>() { nextQuestion } };
        var match = new QuizDev.Core.Entities.Match { Id = Guid.NewGuid(),Quiz = quiz , UserId = userId, Status = QuizDev.Core.Enums.EMatchStatus.Created };

        _matchRepositoryMock.Setup(x => x.GetAsync(match.Id)).ReturnsAsync(match);
        _matchRepositoryMock.Setup(x => x.GetNextQuestion(match)).ReturnsAsync(nextQuestion);

        //Act
        var result = await _useCase.Execute(match.Id, userId);

        //Assert
        var data = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(result.Data));
        Assert.True((bool)data.IsLastQuestion);
        Assert.Equal(QuizDev.Core.Enums.EMatchStatus.Created, match.Status);
    }

    [Fact]
    public async Task Execute_NoNextQuestion_AndResponseCountEqual0_ReturnsFirstQuestion()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var nextQuestion = new Question { Id = Guid.NewGuid(), Order = 0, Text = "", Options = [], QuizId = Guid.NewGuid() };
        var quiz = new Quiz { Questions = new List<Question>() { new(), new() } };
        var match = new QuizDev.Core.Entities.Match { Id = Guid.NewGuid(), Quiz = quiz, UserId = userId, Status = QuizDev.Core.Enums.EMatchStatus.Created, Responses = new List<Response>() };

        _matchRepositoryMock.Setup(x => x.GetAsync(match.Id)).ReturnsAsync(match);
        _questionRepositoryMock.Setup(x => x.GetQuizQuestionByOrder(match.QuizId, 0)).ReturnsAsync(nextQuestion);

        //Act
        var result = await _useCase.Execute(match.Id, userId);

        //Assert
        Assert.Equal(QuizDev.Core.Enums.EMatchStatus.Created, match.Status);
    }

    [Fact]
    public async Task Execute_MatchFinished_ThrowsInvalidOperationException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var nextQuestion = new Question { Id = Guid.NewGuid(), Order = 0, Text = "", Options = [], QuizId = Guid.NewGuid() };
        var quiz = new Quiz { Questions = new List<Question>() { new (), new () } };
        var match = new QuizDev.Core.Entities.Match { UserId = userId, Status = QuizDev.Core.Enums.EMatchStatus.Finished };

        _matchRepositoryMock.Setup(x => x.GetAsync(match.Id)).ReturnsAsync(match);

        //Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(match.Id, userId));
    }

    [Fact]
    public async Task Execute_MatchFailed_ThrowsInvalidOperationException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var nextQuestion = new Question { Id = Guid.NewGuid(), Order = 0, Text = "", Options = [], QuizId = Guid.NewGuid() };
        var quiz = new Quiz { Questions = new List<Question>() { new(), new() } };
        var match = new QuizDev.Core.Entities.Match { UserId = userId, Status = QuizDev.Core.Enums.EMatchStatus.Failed };

        _matchRepositoryMock.Setup(x => x.GetAsync(match.Id)).ReturnsAsync(match);

        //Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(match.Id, userId));
    }

    [Fact]
    public async Task Execute_ItsNotWhoCreatedTheMatch_ThrowsUnauthorizedAccessException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var nextQuestion = new Question { Id = Guid.NewGuid(), Order = 0, Text = "", Options = [], QuizId = Guid.NewGuid() };
        var quiz = new Quiz { Questions = new List<Question>() { new(), new() } };
        var match = new QuizDev.Core.Entities.Match { UserId = Guid.NewGuid(), Status = QuizDev.Core.Enums.EMatchStatus.Failed };

        _matchRepositoryMock.Setup(x => x.GetAsync(match.Id)).ReturnsAsync(match);

        //Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.Execute(match.Id, userId));
    }
}
