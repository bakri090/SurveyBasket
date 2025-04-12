namespace SurveyBasket.Api.Authentication;

public record RefreshTokenRequest(string token, string refreshToken);
