using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SurveyBasket.Api.Contracts.Votes;

namespace SurveyBasket.Api.Controllers;

[ApiVersion(1, Deprecated = true)]
[ApiVersion(2)]
[Route("api/polls/{pollId}/vote")]
[ApiController]
[Authorize(Roles = DefaultRoles.Member.Name)]
[EnableRateLimiting(RateLimiter.UserLimit)]
public class VotesController(IQuestionServices questionServices, IVoteServices voteServices) : ControllerBase
{
	private readonly IQuestionServices _questionServices = questionServices;
	private readonly IVoteServices _voteServices = voteServices;

	[HttpGet("")]
	[ResponseCache(Duration = 60)]
	public async Task<IActionResult> Start([FromRoute] int pollId, CancellationToken cancellationToken)
	{
		var userId = "8e58cdf2-e1a1-47f7-a6cb-67f0b0806f54";//User.GetUserId();
		var result = await _questionServices.GetAvailableAsync(pollId, userId!, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}
	[HttpPost("")]
	public async Task<IActionResult> Vote([FromRoute] int pollId, [FromBody] VoteRequest request,
		CancellationToken cancellationToken)
	{
		var result = await _voteServices.AddAsync(pollId, User.GetUserId()!, request, cancellationToken);

		if (result.IsSuccess) return Created();

		return result.ToProblem();
	}
}
