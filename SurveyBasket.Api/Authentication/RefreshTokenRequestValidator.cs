namespace SurveyBasket.Api.Authentication;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
	public RefreshTokenRequestValidator()
	{
		RuleFor(x => x.token).NotEmpty();
		RuleFor(x => x.refreshToken).NotEmpty();
	}
}
