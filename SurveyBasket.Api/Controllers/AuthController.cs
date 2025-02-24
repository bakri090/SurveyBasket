using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Contracts.Authentication;

namespace SurveyBasket.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController(IAuthService authService,IOptions<JwtOptions> jwtOption) : ControllerBase
{
	private readonly IAuthService _authService = authService;
	private readonly JwtOptions _jwtOption = jwtOption.Value;

	[HttpPost("")]
	public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest,CancellationToken cancellationToken)
	{
		var authResponse = await _authService.GetTokenAsync(loginRequest.Email, loginRequest.Password,cancellationToken);

		return authResponse is null ? BadRequest("email or password is invalid") :Ok(authResponse) ;
	}
	[HttpPost("refresh")]
	public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
	{
		var authResponse = await _authService.GetRefreshTokenAsync(request.token, request.refreshToken, cancellationToken);

		return authResponse is null ? BadRequest("Invalid token") : Ok(authResponse);
	}
	[HttpPut("revoke-refresh-token")]
	public async Task<IActionResult> RevokeRefreshAsync( [FromBody]RefreshTokenRequest request, CancellationToken cancellationToken)
	{
		var isRevoke = await _authService.RevokeRefreshTokenAsync(request.token, request.refreshToken, cancellationToken);

		return isRevoke  ? Ok(): BadRequest("Invalid token")  ;
	}
}
