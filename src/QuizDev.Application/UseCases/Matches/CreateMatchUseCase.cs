﻿

using QuizDev.Application.Exceptions;
using QuizDev.Core.DTOs.AnswerOptions;
using QuizDev.Core.DTOs.Questions;
using QuizDev.Core.DTOs.Responses;
using QuizDev.Core.Entities;
using QuizDev.Core.Repositories;

namespace QuizDev.Application.UseCases.Matches;

public class CreateMatchUseCase
{
    private readonly IMatchRepository _matchRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionRepository _questionRepository;

    public CreateMatchUseCase(IMatchRepository matchRepository, IQuizRepository quizRepository, IQuestionRepository questionRepository)
    {
        _matchRepository = matchRepository;
        _quizRepository = quizRepository;
        _questionRepository = questionRepository;
    }

    public async Task<ResultDto> Execute(Guid quizId, Guid userId)
    {
        var quiz = await _quizRepository.GetAsync(quizId, false);
        if (quiz == null)
        {
            throw new NotFoundException("Quiz não encontrado");
        }

        if (quiz.IsActive == false)
        {
            throw new InvalidOperationException("Não é possível criar uma partida pois o Quiz está inativo");
        }

        var match = new Match
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            Quiz = quiz,
            Reviewed = false,
            Score = 0,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = Core.Enums.EMatchStatus.Created
        };

        await _matchRepository.CreateAsync(match);

        //Pegar a questão na ordem (Question.Order) 0
        var nextQuestion = await _questionRepository.GetQuizQuestionByOrder(quizId, 0);
        if (nextQuestion == null)
        {
            throw new InvalidOperationException("Não foi possível obter a primeira questão do Quiz pois a ordem das questões é inválida");
        }

        var questionDto = new GetQuestionDto
        {
            Id = nextQuestion.Id,
            Text = nextQuestion.Text,
            QuizId = nextQuestion.QuizId,
            Order = nextQuestion.Order,
            Options = nextQuestion.Options.Select(o => new GetAnswerOptionDto(o.Id, o.Response, o.QuestionId)).ToList()
        };

        return new ResultDto(new { MatchId = match.Id, match.ExpiresIn, NextQuestion = questionDto });
    }
}
