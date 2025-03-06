namespace SurveyBasket.Api.Errors;

public static class UserError
{
	public static Error InvalidCredentials =
		new("User.InvalidCredentials", "Invalid email/password",StatusCode:StatusCodes.Status401Unauthorized);

	public static readonly Error InvalidJwtToken =
		new("User.InvalidJwtToken", "Invalid Jwt token", StatusCode: StatusCodes.Status401Unauthorized);

	public static readonly Error InvalidRefreshToken =
		new("User.InvalidRefreshToken", "Invalid refresh token", StatusCode: StatusCodes.Status401Unauthorized);
}
