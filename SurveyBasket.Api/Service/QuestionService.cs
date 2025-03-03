using SurveyBasket.Api.Contracts.Answers;
using SurveyBasket.Api.Contracts.Question;

namespace SurveyBasket.Api.Service;

public class QuestionService(ApplicationDbContext db) : IQuestionService
{
	private readonly ApplicationDbContext _db = db;

	public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int polId, CancellationToken cancellationToken = default)
	{
		var pollIsExists =await _db.Polls.AnyAsync(x => x.Id == polId, cancellationToken: cancellationToken);
		if(!pollIsExists)
			return Result.Failure<IEnumerable<QuestionResponse>>(PollError.PollNotFound);
		
		var questions = await _db.Questions
			.Where(x => x.PollId == polId)
			.Include(x => x.Answers)
			// لو شغال على مشروع كبير فالطريقه دي افضل عشان بتعمل اختيار للاعمده من قاعده البيانات اول وترجعه
			//.Select(q => new QuestionResponse(
			//	 q.Id,
			//	q.Content,
			//	q.Answers.Select(x => new AnswerResponse(x.Id,x.Content))
			//))
			//  دي بترجع الاعمده كلها وتاني بتطلع الاعمده ف الاداء اقل من القبلها
			.ProjectToType<QuestionResponse>()
			.AsNoTracking()
			.ToListAsync(cancellationToken);

		return Result.Success<IEnumerable<QuestionResponse>>(questions);
	}
	public async Task<Result<QuestionResponse>> GetAsync(int polId, int id, CancellationToken cancellationToken = default)
	{
		var question = await _db.Questions
			.Where(x => x.PollId == polId && x.Id == id)
			.Include(x => x.Answers)
			.Select(x => new QuestionResponse
			(x.Id,
			 x.Content,
			 x.Answers.Select(a => new AnswerResponse ( a.Id,  a.Content ))
			 ))
			.AsNoTracking()
			.SingleOrDefaultAsync(cancellationToken: cancellationToken);
		
		if(question is null)
			return Result.Failure<QuestionResponse>(QuestionError.QuestionNotFound);

		return Result.Success<QuestionResponse>(question);
	}
	public async Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionRequest request,
		CancellationToken cancellationToken = default)
	{
		var pollIsExists = await _db.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);

		if(!pollIsExists)
			return Result.Failure<QuestionResponse>(PollError.PollNotFound);
		
		var questionIsExists = await _db.Questions.AnyAsync(x => x.Content == request.Content && x.PollId == pollId, cancellationToken: cancellationToken);
		
		if(questionIsExists)
			return Result.Failure<QuestionResponse>(QuestionError.DuplicatedQuestionContent);
		
		var question = request.Adapt<Question>();
		question.PollId = pollId;

		//request.Answers.ForEach(answer => question.Answers.Add(new Answer { Content = answer }));

		await _db.AddAsync(question);
		await _db.SaveChangesAsync(cancellationToken);

		return Result.Success(question.Adapt<QuestionResponse>());
	}
	public async Task<Result> UpdateAsync(int pollId, int id,QuestionRequest request, CancellationToken cancellationToken = default)
	{
		var questionIsExists =await _db.Questions.AnyAsync(x => x.PollId == pollId
			&& x.Id != id
			&& x.Content == request.Content,
			cancellationToken: cancellationToken);

		if(questionIsExists)
			return Result.Failure(QuestionError.DuplicatedQuestionContent);
		
		var question = await _db.Questions.Include(x => x.Answers)
			.SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == id,cancellationToken);

		if(question is null)
			return Result.Failure(QuestionError.QuestionNotFound);

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
		return Result.Success();
	}
	public async Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default)
	{
		var question = await _db.Questions.SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == id, cancellationToken);
		if( question is null )
			return Result.Failure(QuestionError.QuestionNotFound);

		question.IsActive = !question.IsActive;

		await _db.SaveChangesAsync(cancellationToken);

		return Result.Success();	
	}

	
}
