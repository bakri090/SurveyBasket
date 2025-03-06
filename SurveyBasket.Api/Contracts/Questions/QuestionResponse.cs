using SurveyBasket.Api.Contracts.Answers;

namespace SurveyBasket.Api.Contracts.Question;

public record QuestionResponse(
	int Id,
	string Content,
	IEnumerable<AnswerResponse> Answers
	);
