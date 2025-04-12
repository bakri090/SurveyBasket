using Asp.Versioning;
using SurveyBasket.Api.Contracts.Common;
using SurveyBasket.Api.Contracts.Question;

namespace SurveyBasket.Api.Controllers;

[ApiVersion(1, Deprecated = true)]
[ApiVersion(2)]
[Route("api/polls/{pollId}/[Controller]")]
[ApiController]
public class QuestionsController(IQuestionServices questionServices) : ControllerBase
{
	private readonly IQuestionServices _questionServices = questionServices;

	[HttpGet("")]
	[HasPermission(Permissions.ReadQuestions)]
	public async Task<IActionResult> GetAll([FromRoute] int pollId, [FromQuery] RequestFilters request, CancellationToken cancellationToken)
	{
		var result = await _questionServices.GetAllAsync(pollId, request, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpGet("{id}")]
	[HasPermission(Permissions.ReadQuestions)]
	public async Task<IActionResult> Get([FromRoute] int pollId, [FromRoute] int id, CancellationToken cancellationToken)
	{
		var result = await _questionServices.GetAsync(pollId, id, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpPost("")]
	[HasPermission(Permissions.AddQuestions)]
	public async Task<IActionResult> Add([FromRoute] int pollId, [FromBody] QuestionRequest request,
		CancellationToken cancellationToken = default)
	{
		var result = await _questionServices.AddAsync(pollId, request, cancellationToken);

		return result.IsSuccess ? CreatedAtAction(nameof(Get), new { pollId, result.Value.Id }, result.Value) :
			result.ToProblem();
	}
	[HttpPut("{id}")]
	[HasPermission(Permissions.UpdateQuestions)]
	public async Task<IActionResult> Update([FromRoute] int pollId, [FromRoute] int id, [FromBody] QuestionRequest request,
		CancellationToken cancellationToken = default)
	{
		var result = await _questionServices.UpdateAsync(pollId, id, request, cancellationToken);

		return result.IsSuccess ? NoContent() :
			result.ToProblem();
	}
	[HttpPut("{id}/toggle-status")]
	[HasPermission(Permissions.UpdateQuestions)]
	public async Task<IActionResult> TogglePublish([FromRoute] int pollId, [FromRoute] int id, CancellationToken cancellationToken)
	{
		var result = await _questionServices.ToggleStatusAsync(pollId, id, cancellationToken);

		return result.IsSuccess
			? NoContent()
			: result.ToProblem();
	}
}
