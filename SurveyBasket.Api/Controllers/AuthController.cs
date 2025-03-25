using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Contracts.Authentication;

namespace SurveyBasket.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController(IAuthService authService,ILogger<AuthController> logger) : ControllerBase
{
	private readonly IAuthService _authService = authService;
	private readonly ILogger<AuthController> _logger = logger;

	[HttpPost("")]
	public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest,CancellationToken cancellationToken)
	{
		_logger.LogInformation("Logging with email: {email} and password : {password}",loginRequest.Email,loginRequest.Password);

		var authResult = await _authService.GetTokenAsync(loginRequest.Email, loginRequest.Password,cancellationToken);

		return authResult.IsSuccess 
		? Ok(authResult.Value)
		:authResult.ToProblem();
	}
	[HttpPost("refresh")]
	
	public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
	{
		var authResponse = await _authService.GetRefreshTokenAsync(request.token, request.refreshToken, cancellationToken);

		return authResponse.IsFailure ? authResponse.ToProblem() : Ok(authResponse.Value);
	}

	[HttpPut("revoke-refresh-token")]
	public async Task<IActionResult> RevokeRefresh( [FromBody]RefreshTokenRequest request, CancellationToken cancellationToken)
	{
		var result = await _authService.RevokeRefreshTokenAsync(request.token, request.refreshToken, cancellationToken);

		return result.IsSuccess ? Ok(): result.ToProblem();
	}
	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
	{
		var result = await _authService.RegisterAsync(request, cancellationToken);

		return result.IsFailure ? result.ToProblem() : Ok();
	}
	[HttpPost("confirm-email")]
	public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
	{
		var result = await _authService.ConfirmEmailAsync(request);

		return result.IsFailure ? result.ToProblem() : Ok();
	}
	[HttpPost("resend-confirm-email")]
	public async Task<IActionResult> ResendConfirmEmail([FromBody] ResendConfirmationEmailRequest request)
	{
		var result = await _authService.ResendConfirmationEmailAsync(request);

		return result.IsFailure ? result.ToProblem() : Ok();
	}
	[HttpPost("forget-password")]
	public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
	{
		var result = await _authService.SendResetPasswordCodeAsync(request.Email);

		return result.IsFailure ? result.ToProblem() : Ok();
	}
	[HttpPost("reset-password")]
	public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
	{
		var result = await _authService.ResetPasswordRequestAsync(request);

		return result.IsFailure ? result.ToProblem() : Ok();
	}
}
