﻿
using QuizDev.Application.Exceptions;
using QuizDev.Core.DTOs.Responses;
using QuizDev.Core.Repositories;

namespace QuizDev.Application.UseCases.Questions;

public class UpdateCorrectOptionUseCase
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerOptionRepository _answerOptionRepository;

    public UpdateCorrectOptionUseCase(IQuestionRepository questionRepository, IAnswerOptionRepository answerOptionRepository)
    {
        _questionRepository = questionRepository;
        _answerOptionRepository = answerOptionRepository;
    }

    public async Task<ResultDto> Execute(Guid questionId, Guid newCorrectOptionId, Guid userId)
    {
        var question = await _questionRepository.GetAsync(questionId);
        if (question == null)
        {
            throw new NotFoundException("Questão não encontrada");
        }

        //Verifica se quem está alterando é quem criou o quiz ( quiz -> question -> option)
        //Se quem criou o quiz, vai ser o mesmo quem criou as questões para o quiz e por sua vez o mesmo que criou as opções para a questão
        if (question.Quiz.UserId != userId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para acessar esse recurso");
        }

        var currentCorrectOption = question.Options.FirstOrDefault(x => x.IsCorrectOption);
        //Remove a opção correta atual
        if (currentCorrectOption != null)
        {
            currentCorrectOption.IsCorrectOption = false;
            await _answerOptionRepository.UpdateAsync(currentCorrectOption);
        }

        //Atualiza a opção correta
        var newCorrectOption = question.Options.FirstOrDefault(x => x.Id == newCorrectOptionId);
        if (newCorrectOption == null)
        {
            throw new NotFoundException("Opção de resposta não encontrada");
        }

        newCorrectOption.IsCorrectOption = true;
        await _answerOptionRepository.UpdateAsync(newCorrectOption);

        return new ResultDto(new { QuestionId = questionId, AnswerOptionId = newCorrectOptionId });
    }
}
