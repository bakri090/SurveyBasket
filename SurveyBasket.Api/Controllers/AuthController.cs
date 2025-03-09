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
	public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest,CancellationToken cancellationToken)
	{
		_logger.LogInformation("Logging with email: {email} and password : {password}",loginRequest.Email,loginRequest.Password);

		var authResult = await _authService.GetTokenAsync(loginRequest.Email, loginRequest.Password,cancellationToken);

		return authResult.IsSuccess 
		? Ok(authResult.Value)
		:authResult.ToProblem();
	}
	[HttpPost("refresh")]
	public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
	{
		var authResponse = await _authService.GetRefreshTokenAsync(request.token, request.refreshToken, cancellationToken);

		return authResponse.IsFailure ? authResponse.ToProblem() : Ok(authResponse.Value);
	}
	[HttpPut("revoke-refresh-token")]
	public async Task<IActionResult> RevokeRefreshAsync( [FromBody]RefreshTokenRequest request, CancellationToken cancellationToken)
	{
		var result = await _authService.RevokeRefreshTokenAsync(request.token, request.refreshToken, cancellationToken);

		return result.IsSuccess ? Ok(): result.ToProblem();
	}
}
