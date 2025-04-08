using SurveyBasket.Api.Contracts.Votes;

namespace SurveyBasket.Api.Services;

public class VoteServices(ApplicationDbContext db) : IVoteServices
{
	private readonly ApplicationDbContext _db = db;

	public async Task<Result> AddAsync(int pollId, string userId, VoteRequest request, CancellationToken cancellationToken)
	{
		var hasVote = await _db.Votes.AnyAsync(x => x.PollId == pollId && x.UserId == userId);
		
		if (hasVote) 
			return Result.Failure(VoteErrors.DuplicatedVote);
		
		var pollIsExist = await _db.Polls.AnyAsync(x => x.Id == pollId && x.IsPublished
		&& x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow));

		if (!pollIsExist)
			return Result.Failure(PollErrors.PollNotFound);

		var availableQuestion = await _db.Questions
			.Where(x => x.PollId == pollId && x.IsActive)
			.Select(x => x.Id)
			.ToListAsync(cancellationToken);
		if (!request.Answers.Select(x => x.QuestionId).SequenceEqual(availableQuestion))
			return Result.Failure(VoteErrors.InvalidQuestions);

		var vote = new Vote
		{
			PollId = pollId,
			UserId = userId,
			VoteAnswers = request.Answers.Adapt<IEnumerable<VoteAnswer>>().ToList(),
		};
		await _db.Votes.AddAsync(vote,cancellationToken);
		await _db.SaveChangesAsync(cancellationToken);
		return Result.Success();
	}
}
