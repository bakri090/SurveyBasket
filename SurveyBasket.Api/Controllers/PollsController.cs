using Microsoft.AspNetCore.Mvc.ModelBinding;
using SurveyBasket.Api.Contracts.Validation;

namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PollsController(IPollService poll) : ControllerBase
{
	private readonly IPollService _pollService = poll;

	[HttpGet]
    public IActionResult Get()
	{
        var polls = _pollService.GetAll();

        var responses = polls.Adapt<IEnumerable<PollResponse>>();
		
        return Ok(responses);    
    }

    [HttpGet("{id}")]
    public IActionResult Get([FromRoute]int id)
    {
        var poll = _pollService.Get(id);
        
        if (poll == null)
            return NotFound();

        var response = poll.Adapt<PollResponse>();
        return poll is null ? NotFound() : Ok(response);
    }

    [HttpPost("")]
    public IActionResult Add([FromBody]CreatePollRequest request)
    {
        
        var newPoll = _pollService.Add(request.Adapt<Poll>());
        return CreatedAtAction(nameof(Add), new { id = newPoll.Id }, newPoll);
    }

    [HttpPut("{id}")]
    public IActionResult Update([FromRoute]int id,[FromBody]CreatePollRequest poll)
    {
        var isUpdated = _pollService.Update(id, poll.Adapt<Poll>());
        if (!isUpdated)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete([FromRoute]int id)
    {
        var isDeleted = _pollService.Delete(id);
        if(!isDeleted)
            return NotFound();
        return NoContent();
    }
}
