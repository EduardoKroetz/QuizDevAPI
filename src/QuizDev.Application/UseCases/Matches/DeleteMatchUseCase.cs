﻿
using QuizDev.Core.DTOs.Responses;
using QuizDev.Application.Exceptions;
using QuizDev.Core.Repositories;

namespace QuizDev.Application.UseCases.Matches;

public class DeleteMatchUseCase
{
    private readonly IMatchRepository _matchRepository;

    public DeleteMatchUseCase(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public async Task<ResultDto> Execute(Guid matchId, Guid userId)
    {
        var match = await _matchRepository.GetAsync(matchId);
        if (match == null)
        {
            throw new NotFoundException("Partida não encontrada");
        }

        if (match.UserId != userId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para acessar esse recurso");
        }

        await _matchRepository.DeleteAsync(match);

        return new ResultDto(new { match.Id });
    }
}
