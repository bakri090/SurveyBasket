using SurveyBasket.Api.Service;

namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PollsController(IPollService poll) : ControllerBase
{
	private readonly IPollService _poll = poll;

	[HttpGet]
    public IActionResult Get()
    {
        return Ok(_poll.GetAll());    
    }
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var poll = _poll.Get(id);
        return poll is null ? NotFound() : Ok(poll);
    }
    [HttpPost("")]
    public IActionResult Add(Poll poll)
    {
        var newPoll = _poll.Add(poll);
        return CreatedAtAction(nameof(Add),new { id=poll.Id }, newPoll);
    }
    [HttpPut("{id}")]
    public IActionResult Update(int id, Poll poll)
    {
        var isUpdtaed = _poll.Update(id,poll);
        if(!isUpdtaed)
            return NotFound();
        return NoContent();
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var isDeleted = _poll.Delete(id);
        if(!isDeleted)
            return NotFound();
        return NoContent();
    }
}
