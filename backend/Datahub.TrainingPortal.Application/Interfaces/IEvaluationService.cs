using Datahub.TrainingPortal.Application.DTOs.Evaluations;

namespace Datahub.TrainingPortal.Application.Interfaces;

public interface IEvaluationService
{
    Task<IReadOnlyList<EvaluationDto>> GetForUserAsync(Guid userId);
    Task<EvaluationDto> CreateAsync(Guid userId, CreateEvaluationRequest request);
    Task<EvaluationDto> UpdateAsync(Guid evaluationId, UpdateEvaluationRequest request);
    Task DeleteAsync(Guid evaluationId);
    Task<IReadOnlyList<RankingEntryDto>> GetRankingsAsync();
}
