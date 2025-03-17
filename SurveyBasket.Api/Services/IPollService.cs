namespace SurveyBasket.Api.Service
{
	public interface IPollService
	{
		Task<IEnumerable<Result<PollResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
		Task<IEnumerable<PollResponse>> GetCurrentAsync(CancellationToken cancellationToken = default);
		Task<Result<PollResponse>> GetAsync(int id,CancellationToken cancellationToken = default);
		Task<Result<PollResponse>> AddAsync(PollRequest request, CancellationToken cancellationToken = default);
		Task<Result> UpdateAsync(int id, PollRequest request,CancellationToken cancellationToken = default);
		Task<Result> DeleteAsync(int id,CancellationToken cancellationToken = default);
		Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default);
	}
}
