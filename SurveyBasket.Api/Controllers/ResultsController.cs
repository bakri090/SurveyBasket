using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SurveyBasket.Api.Controllers;
[Route("api/polls/{pollId}/[controller]")]
[ApiController]
[Authorize]
[HasPermission(Permissions.ReadResults)]
public class ResultsController(IResultServices resultServices) : ControllerBase
{
	private readonly IResultServices _resultServices = resultServices;

	[HttpGet("row-data")]
	public async Task<IActionResult> PollVotes([FromRoute] int pollId,CancellationToken cancellationToken)
	{
		var result = await _resultServices.GetPollVotesAsync(pollId,cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}
	[HttpGet("votes-per-day")]
	public async Task<IActionResult> VotesPerDay([FromRoute] int pollId, CancellationToken cancellationToken)
	{
		var result = await _resultServices.GetVotesPerDayAsync(pollId, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}
	[HttpGet("votes-per-question")]
	public async Task<IActionResult> VotesPerQuestion([FromRoute] int pollId, CancellationToken cancellationToken)
	{
		var result = await _resultServices.GetVotesPerQuestionAsync(pollId, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}
}
