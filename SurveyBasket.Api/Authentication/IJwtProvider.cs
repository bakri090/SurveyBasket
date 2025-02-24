namespace SurveyBasket.Api.Authentication;

public interface IJwtProvider
{
	public (string token, int expiresIn) GenerateToken(ApplicationUser user);
	string? ValidateToken(string token);
}
