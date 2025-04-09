using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Contracts.Authentication;

namespace SurveyBasket.Api.Controllers;

[Route("[controller]")]
[ApiController]
[EnableRateLimiting(RateLimiter.IpLimit)]
public class AuthController(IAuthServices authServices,ILogger<AuthController> logger) : ControllerBase
{
	private readonly IAuthServices _authServices = authServices;
	private readonly ILogger<AuthController> _logger = logger;

	[HttpPost("")]
	public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest,CancellationToken cancellationToken)
	{
		_logger.LogInformation("Logging with email: {email} and password : {password}",loginRequest.Email,loginRequest.Password);

		var authResult = await _authServices.GetTokenAsync(loginRequest.Email, loginRequest.Password,cancellationToken);

		return authResult.IsSuccess 
		? Ok(authResult.Value)
		:authResult.ToProblem();
	}
	[HttpPost("refresh")]
	
	public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
	{
		var authResponse = await _authServices.GetRefreshTokenAsync(request.token, request.refreshToken, cancellationToken);

		return authResponse.IsFailure ? authResponse.ToProblem() : Ok(authResponse.Value);
	}

	[HttpPut("revoke-refresh-token")]
	public async Task<IActionResult> RevokeRefresh( [FromBody]RefreshTokenRequest request, CancellationToken cancellationToken)
	{
		var result = await _authServices.RevokeRefreshTokenAsync(request.token, request.refreshToken, cancellationToken);

		return result.IsSuccess ? Ok(): result.ToProblem();
	}
	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
	{
		var result = await _authServices.RegisterAsync(request, cancellationToken);

		return result.IsFailure ? result.ToProblem() : Ok();
	}
	[HttpPost("confirm-email")]
	public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
	{
		var result = await _authServices.ConfirmEmailAsync(request);

		return result.IsFailure ? result.ToProblem() : Ok();
	}
	[HttpPost("resend-confirm-email")]
	public async Task<IActionResult> ResendConfirmEmail([FromBody] ResendConfirmationEmailRequest request)
	{
		var result = await _authServices.ResendConfirmationEmailAsync(request);

		return result.IsFailure ? result.ToProblem() : Ok();
	}
	[HttpPost("forget-password")]
	public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
	{
		var result = await _authServices.SendResetPasswordCodeAsync(request.Email);

		return result.IsFailure ? result.ToProblem() : Ok();
	}
	[HttpPost("reset-password")]
	public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
	{
		var result = await _authServices.ResetPasswordRequestAsync(request);

		return result.IsFailure ? result.ToProblem() : Ok();
	}
	
}
