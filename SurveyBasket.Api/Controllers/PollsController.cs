using Microsoft.AspNetCore.Authorization;
using SurveyBasket.Api.Contracts.Polls;

namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PollsController(IPollService poll) : ControllerBase
{
	private readonly IPollService _pollService = poll;

	[HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
	{
        var polls =  await _pollService.GetAllAsync(cancellationToken);

        var responses = polls.Adapt<IEnumerable<PollResponse>>();
		
        return Ok(responses);    
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute]int id, CancellationToken cancellationToken)
    {
        var poll = await _pollService.GetAsync(id,cancellationToken);
        
        if (poll == null)
            return NotFound();

        var response = poll.Adapt<PollResponse>();
        return poll is null ? NotFound() : Ok(response);
    }

    [HttpPost("")]
    public async Task<IActionResult> Add([FromBody] PollRequest request,
        CancellationToken cancellationToken)
     {

         var newPoll = await _pollService.AddAsync(request.Adapt<Poll>(),cancellationToken);
        return CreatedAtAction(nameof(Add), new { id = newPoll.Id }, newPoll);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PollRequest poll,CancellationToken cancellationToken)
    {
        var isUpdated =await _pollService.UpdateAsync(id, poll.Adapt<Poll>(),cancellationToken);
        
        if (!isUpdated)
            return NotFound();
        
        return NoContent();
    }

    [HttpDelete("{id}")]

    public async Task<IActionResult> Delete([FromRoute] int id,CancellationToken cancellationToken )
    {
        var isDeleted =await _pollService.DeleteAsync(id,cancellationToken);
        
        if (!isDeleted)
            return NotFound();
        
        return NoContent();
    }
    [HttpPut("{id}/togglePublish")]
    public async Task<IActionResult> TogglePublish([FromRoute] int id,CancellationToken cancellationToken)
    {
        var poll = await _pollService.TogglePublishStatusAsync(id,cancellationToken);
       
        if (!poll)
            return NotFound();

        return NoContent();
    }
}
