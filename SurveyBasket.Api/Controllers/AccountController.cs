using SurveyBasket.Api.Contracts.Users;
using SurveyBasket.Api.Services;

namespace SurveyBasket.Api.Controllers
{
	[Route("me")]
	[ApiController]
	[Authorize]
	public class AccountController(IUserService userService) : ControllerBase
	{
		private readonly IUserService _userService = userService;

		[HttpGet("")]
		public async Task<IActionResult> Info()
		{
			var result = await _userService.GetUserProfileAsync(User.GetUserId()!);
			return Ok(result.Value); 
		}

		[HttpPut("update")]
		public async Task<IActionResult> Info([FromBody] UpdateProfileRequest request)
		{
			var result = await _userService.UpdateProfileAsync(User.GetUserId()!, request);

			return NoContent();
		}

		[HttpPut("change-password")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
		{
			var result = await _userService.ChangePasswordAsync(User.GetUserId()!, request);

			return result.IsSuccess ? NoContent() : result.ToProblem();
		}
	}
}
