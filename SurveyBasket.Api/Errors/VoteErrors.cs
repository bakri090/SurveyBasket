namespace SurveyBasket.Api.Errors;

public abstract class VoteErrors
{
	public static readonly Error InvalidQuestions =
		new("Vote.InvalidQuestions","Invalid Question",StatusCodes.Status400BadRequest);
	
	public static readonly Error DuplicatedVote = 
		new("Vote.DuplicatedVote", "This user already voted before for this poll",StatusCodes.Status409Conflict);
}
