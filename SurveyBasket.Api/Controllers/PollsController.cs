using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;

namespace SurveyBasket.Api.Controllers;

[ApiVersion(1, Deprecated = true)]
[ApiVersion(2)]
[Route("api/[controller]")]
[ApiController]
public class PollsController(IPollServices pollServices) : ControllerBase
{
	private readonly IPollServices _pollServices = pollServices;

	[HttpGet("")]
    [HasPermission(Permissions.ReadPolls)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
	{
        var polls = await _pollServices.GetAllAsync(cancellationToken);
        var results = polls.Select(x => x.Value).ToList();
		
        return Ok(results) ;    
    }

	[MapToApiVersion(1)]
	[HttpGet("current")]
    [Authorize(Roles = DefaultRoles.Member)]
    [EnableRateLimiting(RateLimiter.UserLimit)]
	public async Task<IActionResult> GetCurrentV1(CancellationToken cancellationToken)
	{
		return Ok(await _pollServices.GetCurrentAsyncV1(cancellationToken));
	}

	[MapToApiVersion(2)]
	[HttpGet("current")]
	[Authorize(Roles = DefaultRoles.Member)]
	[EnableRateLimiting(RateLimiter.UserLimit)]
	public async Task<IActionResult> GetCurrentV2(CancellationToken cancellationToken)
	{
		return Ok(await _pollServices.GetCurrentAsyncV2(cancellationToken));
	}

	[HttpGet("{id}")]
	[HasPermission(Permissions.ReadPolls)]
	public async Task<IActionResult> Get([FromRoute]int id, CancellationToken cancellationToken)
    {
        var poll = await _pollServices.GetAsync(id,cancellationToken);
        
        return poll.IsSuccess ? Ok(poll.Value) : poll.ToProblem();
    }
	[HttpPost("")]
	[HasPermission(Permissions.AddPolls)]
	public async Task<IActionResult> Add([FromBody] PollRequest request,
		CancellationToken cancellationToken)
	{
		var result = await _pollServices.AddAsync(request, cancellationToken);

		return result.IsSuccess? CreatedAtAction(nameof (Get), new { id = result.Value.Id }, result.Value) : result.ToProblem();
	}

	[HttpPut("{id}")]
	[HasPermission(Permissions.UpdatePolls)]
	public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PollRequest request,CancellationToken cancellationToken)
    {
        var result =await _pollServices.UpdateAsync(id, request ,cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeletePolls)]
	public async Task<IActionResult> Delete([FromRoute] int id,CancellationToken cancellationToken )
    {
        var result =await _pollServices.DeleteAsync(id,cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
	}
    [HttpPut("{id}/toggle-publish")]
    [HasPermission(Permissions.UpdatePolls)]
    public async Task<IActionResult> TogglePublish([FromRoute] int id,CancellationToken cancellationToken)
    {
        var result = await _pollServices.TogglePublishStatusAsync(id,cancellationToken);

        return result.IsSuccess ? NoContent() :
            result.ToProblem();
	}
}
