namespace SurveyBasket.Api.Contracts.Validation;

public class PollRequestValidator : AbstractValidator<PollRequest>
{
	public PollRequestValidator()
	{
		RuleFor(x => x.Title).NotEmpty()
			.WithMessage("Please add a {PropertyName}")
			.Length(3, 100)
			.WithMessage("Title should be at least {MinLength}, and maximum {MaxLength}, you entered {TotalLength}");
		RuleFor(x => x.Summary).NotEmpty()
			.WithMessage("Please add a {PropertyName}")
			.Length(3, 100)
			.WithMessage("Title should be at least {MinLength}, and maximum {MaxLength}, you entered {TotalLength}");
		RuleFor(x => x.StartsAt)
			.NotEmpty()
			.GreaterThanOrEqualTo(x => DateOnly.FromDateTime(DateTime.Today));
		
		RuleFor(x => x.EndsAt).NotEmpty();

		RuleFor(x => x)
			.Must(HasValidDates)
			.WithName(nameof(PollRequest.EndsAt))
			.WithMessage("{PropertyName} must be greater than or equals starts date");
	}
	private bool HasValidDates(PollRequest pollRequest)
	{
		return pollRequest.EndsAt >= pollRequest.StartsAt;
	}
}
