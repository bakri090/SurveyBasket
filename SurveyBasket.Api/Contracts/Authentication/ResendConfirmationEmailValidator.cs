namespace SurveyBasket.Api.Contracts.Authentication;

public class ResendConfirmationEmailValidator : AbstractValidator<ResendConfirmationEmailRequest>
{
	public ResendConfirmationEmailValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty()
			.EmailAddress();
	}
}
