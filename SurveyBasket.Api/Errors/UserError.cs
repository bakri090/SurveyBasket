namespace SurveyBasket.Api.Errors;

public static class UserError
{
	public static Error InvalidCredentials =
		new("User.InvalidCredentials", "Invalid email/password",StatusCode:StatusCodes.Status401Unauthorized);

	public static readonly Error InvalidJwtToken =
		new("User.InvalidJwtToken", "Invalid Jwt token", StatusCode: StatusCodes.Status401Unauthorized);

	public static readonly Error InvalidRefreshToken =
		new("User.InvalidRefreshToken", "Invalid refresh token", StatusCode: StatusCodes.Status401Unauthorized);

	public static readonly Error DuplicatedEmail =
		new("User.DuplicatedEmail", "Another user with the same email is already exists", StatusCode: StatusCodes.Status409Conflict);

	public static readonly Error EmailNotConfirmed =
		new("User.EmailNotConfirmed", "Email is not confirmed", StatusCode: StatusCodes.Status401Unauthorized);

	public static readonly Error InvalidCode =
		new("User.InvalidCode", "Invalid Code", StatusCode: StatusCodes.Status401Unauthorized);

	public static readonly Error DuplicatedConfirmation =
		new("User.DuplicatedConfirmation", "Email already Confirmed", StatusCode: StatusCodes.Status401Unauthorized);
}
