﻿using Microsoft.AspNetCore.Identity;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Errors;
using System.Security.Cryptography;

namespace SurveyBasket.Api.Service;

public class AuthService(UserManager<ApplicationUser> userManager,IJwtProvider jwtProvider) : IAuthService
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly IJwtProvider _jwtProvider = jwtProvider;


	private readonly int _refreshTokenExpiryDays = 14;

	public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, 
		 CancellationToken cancellationToken = default)
	{
		var userId = _jwtProvider.ValidateToken(token);
		if (userId == null)
			return Result.Failure<AuthResponse>(UserError.InvalidCredentials);

		var user = await _userManager.FindByIdAsync(userId);

		if (user == null)
			return Result.Failure<AuthResponse>(UserError.InvalidCredentials);
			
		
		var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);
		if (userRefreshToken == null)
			return Result.Failure<AuthResponse>(UserError.InvalidCredentials);
			
		userRefreshToken.RevokedOn = DateTime.UtcNow;

		var (newToken , expiresIn) = _jwtProvider.GenerateToken(user);
		// refresh Token
		var newRefreshToken = GenerateRefreshToken();
		var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

		user.RefreshTokens.Add(new RefreshToken
		{
			Token = newRefreshToken,
			ExpiresOn = refreshTokenExpiration,
		});
		await _userManager.UpdateAsync(user);
		var authResponse = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, newToken, expiresIn, newRefreshToken, refreshTokenExpiration);
		return Result.Success(authResponse);
	}

	public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password,
		CancellationToken cancellationToken = default)
	{
		// check user?
		var user = await _userManager.FindByEmailAsync(email);
		if (user is null)
			return Result.Failure<AuthResponse>(UserError.InvalidCredentials);

		// check password
		var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
		if (!isValidPassword) 
			return Result.Failure<AuthResponse>(UserError.InvalidCredentials);

		// generate token
		var (token, expiresIn) = _jwtProvider.GenerateToken(user);
		// refresh Token
		var refreshToken = GenerateRefreshToken();
		var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

		user.RefreshTokens.Add(new RefreshToken
		{
			Token =refreshToken,
			ExpiresOn = refreshTokenExpiration,
		});
		await _userManager.UpdateAsync(user);

		var response = new AuthResponse(user.Id, user.Email!, user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExpiration);
		
		return Result.Success(response);
	}

	public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
	{
		var userId = _jwtProvider.ValidateToken(token);
		if (userId == null)
			return Result.Failure<ApplicationUser>(UserError.InvalidCredentials);

		var user = await _userManager.FindByIdAsync(userId);

		if (user == null)
			return Result.Failure<ApplicationUser>(UserError.InvalidCredentials);

		var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);
		if (userRefreshToken == null)
			return Result.Failure<ApplicationUser>(UserError.InvalidCredentials);
			
		userRefreshToken.RevokedOn = DateTime.UtcNow;
		await _userManager.UpdateAsync(user);

		userRefreshToken.RevokedOn = DateTime.UtcNow;

		return Result.Success(userRefreshToken);
	}

	private string GenerateRefreshToken()
	{
		return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
	}
}
