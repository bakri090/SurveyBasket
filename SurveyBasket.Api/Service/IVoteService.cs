using SurveyBasket.Api.Contracts.Votes;

namespace SurveyBasket.Api.Service;

public interface IVoteService
{
	public Task<Result> AddAsync(int pollId,string userId,VoteRequest request,CancellationToken cancellationToken);
}
