namespace SurveyBasket.Api.Errors;

public abstract class PollError
{
	public static readonly Error PollNotFound = new("PollNotFound","No poll was found with the given ID",StatusCodes.Status404NotFound  );
	public static readonly Error DuplicatedPollTitle = new("Poll.DuplicatedTitle", "Another poll with the same title is already exists",StatusCodes.Status409Conflict);
}
