namespace SurveyBasket.Api.Contracts.Validation;

public class CreatePollRequestValidator : AbstractValidator<CreatePollRequest>
{
	public CreatePollRequestValidator()
	{
		RuleFor(x => x.Title).NotEmpty()
			.WithMessage("Please add a {PropertyName}")
			.Length(3, 100)
			.WithMessage("Title should be at least {MinLength}, and maximum {MaxLength}, you entered {TotalLength}");
		RuleFor(x => x.Description).NotEmpty()
			.WithMessage("Please add a {PropertyName}")
			.Length(3, 100)
			.WithMessage("Title should be at least {MinLength}, and maximum {MaxLength}, you entered {TotalLength}");
	}
}
