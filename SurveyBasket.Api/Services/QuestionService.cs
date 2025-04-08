using System.Linq.Dynamic.Core;
using SurveyBasket.Api.Contracts.Answers;
using SurveyBasket.Api.Contracts.Common;
using SurveyBasket.Api.Contracts.Question;

namespace SurveyBasket.Api.Services;

public class QuestionServices(ApplicationDbContext db,ICacheServices cacheServices,ILogger<QuestionServices> logger) : IQuestionServices
{
	private readonly ApplicationDbContext _db = db;
	private readonly ICacheServices _cacheServices = cacheServices;
	private readonly ILogger<QuestionServices> _logger = logger;

	private const string _cachePrefix = "availableQuestions"; 
	public async Task<Result<PaginatedList<QuestionResponse>>> GetAllAsync(int pollId,RequestFilters request, CancellationToken cancellationToken = default)
	{
		var pollIsExists =await _db.Polls.AnyAsync(x => x.Id == pollId, cancellationToken: cancellationToken);
		if (!pollIsExists)
			return Result.Failure<PaginatedList<QuestionResponse>>(PollErrors.PollNotFound);

		var query = _db.Questions
			.Where(x => x.PollId == pollId &&
			(string.IsNullOrEmpty(request.SearchValue) || x.Content.Contains(request.SearchValue)));
			
		if(!string.IsNullOrEmpty(request.SortColumn))
		{
			query = query.OrderBy($"{request.SortColumn} {request.SortDirection}");
		}
		var source = query.Include(x => x.Answers)
			// لو شغال على مشروع كبير فالطريقه دي افضل عشان بتعمل اختيار للاعمده من قاعده البيانات اول وترجعه
			//.Select(q => new QuestionResponse(
			//	 q.Id,
			//	q.Content,
			//	q.Answers.Select(x => new AnswerResponse(x.Id,x.Content))
			//))
			//  دي بترجع الاعمده كلها وتاني بتطلع الاعمده ف الاداء اقل من القبلها
			.ProjectToType<QuestionResponse>()
			.AsNoTracking();

		var questions = await PaginatedList<QuestionResponse>.CreateAsync(source, request.PageNumber,request.PageSize,cancellationToken);

		return Result.Success(questions);
	}
	public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int pollId, string userId,
		CancellationToken cancellationToken = default)
	{
		//var hasVote = await _db.Votes.AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken);
		
		//if (hasVote)
		//	return Result.Failure<IEnumerable<QuestionResponse>>(VoteError.DuplicatedVote);

		//var pollIsExist = await _db.Polls.AnyAsync( x => x.Id == pollId && x.IsPublished && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow) ,cancellationToken);

		//if (!pollIsExist)
		//return Result.Failure<IEnumerable<QuestionResponse>>(PollError.PollNotFound);

		var cacheKey = $"{_cachePrefix}-{pollId}";
		var cacheQuestions = await _cacheServices.GetAsync<IEnumerable<QuestionResponse>>(cacheKey,cancellationToken);
		IEnumerable<QuestionResponse> questions = [];

		if (cacheQuestions is null)
		{
			_logger.LogInformation("Select question from database");
			questions = await _db.Questions.Where(x => x.PollId == pollId && x.IsActive)
				.Include(x => x.Answers)
				.Select(q => new QuestionResponse(
					q.Id,
					q.Content,
					q.Answers.Where(a => a.IsActive).Select(x => new AnswerResponse(x.Id, x.Content))
					))
					.AsNoTracking()
					.ToListAsync(cancellationToken);
			await _cacheServices.SetAsync(cacheKey, questions,cancellationToken);
		}
		else
		{
			_logger.LogInformation("Select question from cache");

			questions = cacheQuestions;
		}
		//var questions = await _db.Questions.Where(x => x.PollId == pollId && x.IsActive)
		//	.Include(x => x.Answers)
		//	.Select(q => new QuestionResponse(
		//		q.Id,
		//		q.Content,
		//		q.Answers.Where(a => a.IsActive).Select(x => new AnswerResponse(x.Id, x.Content))))
		//	.AsNoTracking()
		//	.ToListAsync(cancellationToken);

		return Result.Success<IEnumerable<QuestionResponse>>(questions);
	}
	public async Task<Result<QuestionResponse>> GetAsync(int pollId, int id, CancellationToken cancellationToken = default)
	{
		var question = await _db.Questions
			.Where(x => x.PollId == pollId && x.Id == id)
			.Include(x => x.Answers)
			.Select(x => new QuestionResponse
			(x.Id,
			 x.Content,
			 x.Answers.Select(a => new AnswerResponse ( a.Id,  a.Content ))
			 ))
			.AsNoTracking()
			.SingleOrDefaultAsync(cancellationToken: cancellationToken);
		
		if (question is null)
			return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

		await _cacheServices.RemoveAsync($"{_cachePrefix}-{pollId}", cancellationToken);
		return Result.Success<QuestionResponse>(question);
	}
	public async Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionRequest request,
		CancellationToken cancellationToken = default)
	{
		var pollIsExists = await _db.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);

		if (!pollIsExists)
			return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);
		
		var questionIsExists = await _db.Questions.AnyAsync(x => x.Content == request.Content && x.PollId == pollId, cancellationToken: cancellationToken);
		
		if (questionIsExists)
			return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);
		
		var question = request.Adapt<Question>();
		question.PollId = pollId;

		//request.Answers.ForEach(answer => question.Answers.Add(new Answer { Content = answer }));

		await _db.AddAsync(question);
		await _db.SaveChangesAsync(cancellationToken);

		await _cacheServices.RemoveAsync($"{_cachePrefix}-{pollId}", cancellationToken);
		return Result.Success(question.Adapt<QuestionResponse>());
	}
	public async Task<Result> UpdateAsync(int pollId, int id,QuestionRequest request, CancellationToken cancellationToken = default)
	{
		var questionIsExists =await _db.Questions.AnyAsync(x => x.PollId == pollId
			&& x.Id != id
			&& x.Content == request.Content,
			cancellationToken: cancellationToken);

		if (questionIsExists)
			return Result.Failure(QuestionErrors.DuplicatedQuestionContent);
		
		var question = await _db.Questions.Include(x => x.Answers)
			.SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == id,cancellationToken);

		if (question is null)
			return Result.Failure(QuestionErrors.QuestionNotFound);

		question.Content = request.Content;

		// current answers  (answer in DB)
		var currentAnswers = question.Answers.Select(x => x.Content).ToList();

		var newAnswers = request.Answers.Except(currentAnswers).ToList();

		newAnswers.ForEach(answer =>
		{
			question.Answers.Add(new Answer { Content = answer });
		});

		question.Answers.ToList().ForEach(answer =>
		{
			answer.IsActive = request.Answers.Contains(answer.Content);
		});
		await _db.SaveChangesAsync(cancellationToken);
		await _cacheServices.RemoveAsync($"{_cachePrefix}-{pollId}", cancellationToken);
		return Result.Success();
	}
	public async Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default)
	{
		var question = await _db.Questions.SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == id, cancellationToken);
		if ( question is null )
			return Result.Failure(QuestionErrors.QuestionNotFound);

		question.IsActive = !question.IsActive;
		await _db.SaveChangesAsync(cancellationToken);

		await _cacheServices.RemoveAsync($"{_cachePrefix}-{pollId}", cancellationToken);
		return Result.Success();	
	}

}
