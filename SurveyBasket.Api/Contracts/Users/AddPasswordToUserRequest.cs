namespace SurveyBasket.Api.Contracts.Users;

public record AddPasswordToUserRequest(
	string Password,
	string Code,
	string Email
	);
