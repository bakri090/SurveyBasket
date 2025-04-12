namespace SurveyBasket.Api.Contracts.Users;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
	public ChangePasswordRequestValidator()
	{
		RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required");

		RuleFor(x => x.NewPassword)
			.NotEmpty()
			.Matches(RegexPatterns.Password)
			.WithMessage("Password should be at least 8 digits and should contains lowercase, NonAlphanumeric and UpperCase")
			.NotEqual(x => x.CurrentPassword)
			.WithMessage("New Password can't be same the current password");
	}
}
