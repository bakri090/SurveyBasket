

namespace SurveyBasket.Api.Services;

public class ResultServices(ApplicationDbContext db) : IResultServices
{
	private readonly ApplicationDbContext _db = db;

	public async Task<Result<PollVotesResponse>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default)
	{
		

		var pollVotes = await _db.Polls.Where(x => x.Id == pollId)
			.Select(x => new PollVotesResponse(
				x.Title,
				x.Votes.Select(v => new VoteResponse(
					$"{v.User.FirstName} {v.User.LastName}",
					v.SubmittedOn,
					v.VoteAnswers.Select(x => new QuestionAnswerResponse(
						x.Question.Content,
						x.Answer.Content
						))
					))
				)).SingleOrDefaultAsync(cancellationToken);

		return pollVotes is null ? Result.Failure<PollVotesResponse>(PollErrors.PollNotFound) : Result.Success(pollVotes);
	}

	public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetVotesPerDayAsync(int pollId,
		CancellationToken cancellationToken = default)
	{
		var pollIsExist = await _db.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);

		if (!pollIsExist)
			return Result.Failure<IEnumerable<VotesPerDayResponse>>(PollErrors.PollNotFound);
		
		var votesPerDay = await _db.Votes.Where(x => x.PollId == pollId)
			.GroupBy(x => new {Date = DateOnly.FromDateTime(x.SubmittedOn)})
			.Select(g => new VotesPerDayResponse(
				g.Key.Date,
				g.Count()
				)).ToListAsync(cancellationToken);

		return Result.Success<IEnumerable<VotesPerDayResponse>>(votesPerDay);
	}

	public async Task<Result<IEnumerable<VotesPerQuestionResponse>>> GetVotesPerQuestionAsync(int pollId,
		CancellationToken cancellationToken = default)
	{
		var pollIsExist = await _db.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);

		if (!pollIsExist)
			return Result.Failure<IEnumerable<VotesPerQuestionResponse>>(PollErrors.PollNotFound);

		var votesPerQuestion = await _db.VoteAnswers
			.Where(x => x.Vote.PollId == pollId)
			.Select(x => new VotesPerQuestionResponse(
				x.Question.Content,
				x.Question.Votes
					.GroupBy(x => new { AnswerId = x.Answer.Id, AnswerContent = x.Answer.Content })
					.Select(g => new VotesPerAnswerResponse(
						g.Key.AnswerContent,
						g.Count()
						)))
				).ToListAsync(cancellationToken);

		return Result.Success<IEnumerable<VotesPerQuestionResponse>>(votesPerQuestion);
	}
}
