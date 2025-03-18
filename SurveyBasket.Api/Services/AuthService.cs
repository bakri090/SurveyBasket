using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Errors;
using SurveyBasket.Api.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace SurveyBasket.Api.Service;

public class AuthService(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager
	,IJwtProvider jwtProvider,
	ILogger<AuthService> logger,
	IEmailSender emailSender,
	IHttpContextAccessor httpContextAccessor) : IAuthService
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
	private readonly IJwtProvider _jwtProvider = jwtProvider;
	private readonly ILogger<AuthService> _logger = logger;
	private readonly IEmailSender _emailSender = emailSender;
	private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
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
		//[1]
		//var user = await _userManager.FindByEmailAsync(email);
		//if (user is null)
		if(await _userManager.FindByEmailAsync(email) is not { } user)	
		return Result.Failure<AuthResponse>(UserError.InvalidCredentials);

		// check password
		//var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
		//if (!isValidPassword) 
		//	return Result.Failure<AuthResponse>(UserError.InvalidCredentials);

		var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

		if (result.Succeeded)
		{
			// generate token
			var (token, expiresIn) = _jwtProvider.GenerateToken(user);
			// refresh Token
			var refreshToken = GenerateRefreshToken();
			var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

			user.RefreshTokens.Add(new RefreshToken
			{
				Token = refreshToken,
				ExpiresOn = refreshTokenExpiration,
			});
			await _userManager.UpdateAsync(user);

			var response = new AuthResponse(user.Id, user.Email!, user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExpiration);

			return Result.Success(response);
		}
		return Result.Failure<AuthResponse>(result.IsNotAllowed ? UserError.EmailNotConfirmed : UserError.InvalidCredentials);
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
	public async Task<Result> RegisterAsync(RegisterRequest registerRequest, CancellationToken cancellationToken = default)
	{
		var emailExists = await _userManager.Users.AnyAsync(x =>  x.Email == registerRequest.Email,cancellationToken);

		if(emailExists)
			return Result.Failure(UserError.DuplicatedEmail);	

		var user = registerRequest.Adapt<ApplicationUser>();

		var result = await _userManager.CreateAsync(user, registerRequest.Password);

		if (result.Succeeded)
		{
			var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

			_logger.LogInformation("Confirmation code: {code}", code);

			//TODO : Send email
			await	SendConfirmationEmail(user, code);

			return Result.Success();
		}
		var error = result.Errors.First();

		return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}
	public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
	{
		if(await _userManager.FindByIdAsync(request.UserId) is not { } user)
			return Result.Failure(UserError.InvalidCode);

		if (user.EmailConfirmed)
			return Result.Failure(UserError.DuplicatedConfirmation);

		var code = request.Code;

		try
		{
			code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
		}
		catch (FormatException)
		{
			return Result.Failure(UserError.InvalidCode);

		}
		var result = await _userManager.ConfirmEmailAsync(user, code);
		
		if (result.Succeeded) 
			return Result.Success();

		var error = result.Errors.First();

		return Result.Failure(new Error(error.Code,error.Description, StatusCodes.Status400BadRequest));
	}
	public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request)
	{
		if(await _userManager.FindByEmailAsync(request.Email) is not {} user )
			return Result.Success();

		if (user.EmailConfirmed)
			return Result.Failure(UserError.DuplicatedConfirmation);

		var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
		code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

		_logger.LogInformation("Confirmation code: {code}", code);

		await SendConfirmationEmail(user, code);

		return Result.Success();
	}
	private string GenerateRefreshToken()
	{
		return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
	}
	private async Task SendConfirmationEmail(ApplicationUser user,string code)
	{
		var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

		var emailBody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
			templateModel: new Dictionary<string, string>
			{
					{"{{name}}",user.FirstName},
					{"{{action_url}}", $"{origin}/auth/emailConfirmation?userId={user.Id}&code={code}"} }
			);
		BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✔️ Survey Basket: Email Confirmation", emailBody));

		await Task.CompletedTask;
	}
	
}
