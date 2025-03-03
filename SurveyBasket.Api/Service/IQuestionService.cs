using SurveyBasket.Api.Contracts.Question;

namespace SurveyBasket.Api.Service;

public interface IQuestionService
{
	Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int polId, CancellationToken cancellationToken = default);
	Task<Result<QuestionResponse>> GetAsync(int polId,int id, CancellationToken cancellationToken = default);
	Task<Result<QuestionResponse>> AddAsync(int pollId,QuestionRequest request,CancellationToken cancellationToken = default);
	Task<Result> UpdateAsync(int pollId, int id, QuestionRequest request, CancellationToken cancellationToken = default);
	Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default);
}
