namespace SurveyBasket.Api.Contracts.Users;

public class AddPasswordToUserRequestValidator : AbstractValidator<AddPasswordToUserRequest>
{
	public AddPasswordToUserRequestValidator()
	{
		RuleFor(x => x.Code).NotEmpty();

		RuleFor(x => x.Email).NotEmpty();
		
		RuleFor(x => x.Password)
			.NotEmpty()
			.Matches(RegexPatterns.Password)
			.WithMessage("Password should be at least 8 digits and should contains lowerCase, NonAlphanumeric and UpperCase");
	}
};
