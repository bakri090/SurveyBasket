﻿using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SurveyBasket.Api.Authentication;

public class JwtProvider(IOptions<JwtOptions> jwtOption) : IJwtProvider
{
	private readonly JwtOptions _jwtOption = jwtOption.Value;

	public (string token, int expiresIn) GenerateToken(ApplicationUser user,IEnumerable<string> roles,IEnumerable<string> permissions)
	{
		Claim[] claims = [
			new(JwtRegisteredClaimNames.Sub,user.Id),
			new(JwtRegisteredClaimNames.Email,user.Email!),
			new(JwtRegisteredClaimNames.GivenName,user.FirstName),
			new(JwtRegisteredClaimNames.FamilyName,user.LastName),
			new(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
			new(nameof (roles),JsonSerializer.Serialize(roles),JsonClaimValueTypes.JsonArray),
			new(nameof (permissions),JsonSerializer.Serialize(permissions),JsonClaimValueTypes.JsonArray),
			];
		var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOption.Key));
		var signingCredentials = new SigningCredentials(symmetricSecurityKey,SecurityAlgorithms.HmacSha256);
		
		var token = new JwtSecurityToken(
			issuer: _jwtOption.Issuer,
			audience: _jwtOption.Audience,
			claims: claims,
			expires : DateTime.UtcNow.AddMinutes(_jwtOption.ExpiryMinutes),
			signingCredentials: signingCredentials
			);
		return (token: new JwtSecurityTokenHandler().WriteToken(token), expiresIn:_jwtOption.ExpiryMinutes * 60);
	}

	public string? ValidateToken(string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var SymmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOption.Key));

		try
		{
			tokenHandler.ValidateToken(token, new TokenValidationParameters
			{
				IssuerSigningKey = SymmetricSecurityKey,
				ValidateIssuerSigningKey = true,
				ValidateIssuer = false,
				ValidateAudience = false,
				ClockSkew = TimeSpan.Zero,
			},out SecurityToken validatedToken);

			var jwtToken = (JwtSecurityToken)validatedToken;

			return jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
		}
		catch 
		{

			return null;
		}
	}
}
