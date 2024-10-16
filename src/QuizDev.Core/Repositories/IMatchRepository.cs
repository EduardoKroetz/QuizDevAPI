﻿


using QuizDev.Core.DTOs.Matches;
using QuizDev.Core.Entities;

namespace QuizDev.Core.Repositories;

public interface IMatchRepository
{
    Task CreateAsync(Match match);
    Task<Question?> GetNextQuestion(Match match);
    Task<GetMatchDto?> GetDetailsAsync(Guid matchId);
    Task<Match?> GetAsync(Guid matchId);
    Task UpdateAsync(Match match);
    Task DeleteAsync(Match match);
    Task<List<GetMatchDto>> GetMatchesAsync(Guid userId, int skip, int take, string? reference = null, string? status = null, bool? reviewed = null, string? orderBy = null);
}
