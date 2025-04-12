﻿namespace SurveyBasket.Api.Authentication;

public interface IJwtProvider
{
	public (string token, int expiresIn) GenerateToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions);
	string? ValidateToken(string token);
}
