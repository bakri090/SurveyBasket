namespace SurveyBasket.Api.Errors;

public abstract class QuestionError
{
	public static readonly Error QuestionNotFound = new("QuestionNotFound", "No Question was found with the given ID", StatusCodes.Status404NotFound  );
	public static readonly Error DuplicatedQuestionContent = new("Question.DuplicatedContent", "Another question with the same content is already exists", StatusCodes.Status409Conflict);
}
