namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PollsController(IPollService poll) : ControllerBase
{
	private readonly IPollService _pollService = poll;

	[HttpGet("")]
    [HasPermission(Permissions.ReadPolls)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
	{
        var polls = await _pollService.GetAllAsync(cancellationToken);
        var results = polls.Select(x => x.Value).ToList();
		
        return Ok(results) ;    
    }
	[HttpGet("current")]
    [Authorize(Roles = DefaultRoles.Member)]
	public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
	{
		return Ok(await _pollService.GetCurrentAsync(cancellationToken));
	}

	[HttpGet("{id}")]
	[HasPermission(Permissions.ReadPolls)]
	public async Task<IActionResult> Get([FromRoute]int id, CancellationToken cancellationToken)
    {
        var poll = await _pollService.GetAsync(id,cancellationToken);
        
        return poll.IsSuccess ? Ok(poll.Value) : poll.ToProblem();
    }
	[HttpPost("")]
	[HasPermission(Permissions.AddPolls)]
	public async Task<IActionResult> Add([FromBody] PollRequest request,
		CancellationToken cancellationToken)
	{
		var result = await _pollService.AddAsync(request, cancellationToken);

		return result.IsSuccess? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value) : result.ToProblem();
	}

	[HttpPut("{id}")]
	[HasPermission(Permissions.UpdatePolls)]
	public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PollRequest request,CancellationToken cancellationToken)
    {
        var result =await _pollService.UpdateAsync(id, request ,cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeletePolls)]
	public async Task<IActionResult> Delete([FromRoute] int id,CancellationToken cancellationToken )
    {
        var result =await _pollService.DeleteAsync(id,cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
	}
    [HttpPut("{id}/toggle-publish")]
    [HasPermission(Permissions.UpdatePolls)]
    public async Task<IActionResult> TogglePublish([FromRoute] int id,CancellationToken cancellationToken)
    {
        var result = await _pollService.TogglePublishStatusAsync(id,cancellationToken);

        return result.IsSuccess ? NoContent() :
            result.ToProblem();
	}
}
