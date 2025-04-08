using SurveyBasket.Api.Contracts.Users;

namespace SurveyBasket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserServices userServices) : ControllerBase
    {
		private readonly IUserServices _userServices = userServices;

        [HttpGet("")]
        [HasPermission(Permissions.ReadUsers)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            return Ok(await _userServices.GetAllAsync(cancellationToken));
        }

		[HttpGet("{id}")]
		[HasPermission(Permissions.ReadUsers)]
		public async Task<IActionResult> Get([FromRoute] string id)
		{
            var result = await _userServices.GetAsync(id);
			return result.IsSuccess? Ok(result.Value) : result.ToProblem() ;
		}

        [HttpPost("")]
        [HasPermission(Permissions.AddUsers)]
        public async Task<IActionResult> Add([FromBody] CreateUserRequest request,CancellationToken cancellationToken)
		{
			var result = await _userServices.AddAsync(request,cancellationToken);

			return result.IsSuccess
                ? CreatedAtAction(nameof (Get),new {result.Value.Id},result.Value)
                : result.ToProblem();
		}
		[HttpPost("add-password")]
		public async Task<IActionResult> AddPasswordToUser([FromBody] AddPasswordToUserRequest request,
			CancellationToken cancellationToken)
		{
			var result = await _userServices.AddPasswordToUserAsync(request, cancellationToken);

			return result.IsSuccess
				? NoContent()
				: result.ToProblem();
		}
		[HttpPut("{id}")]
		[HasPermission(Permissions.UpdateUsers)]
		public async Task<IActionResult> Update([FromRoute] string id,[FromBody] UpdateUserRequest request,CancellationToken cancellationToken)
		{
			var result = await _userServices.UpdateAsync(id, request, cancellationToken);

			return result.IsSuccess ? NoContent() : result.ToProblem();
		}

		[HttpPut("{id}/toggle-status")]
		public async Task<IActionResult> ToggleStatus([FromRoute] string id, CancellationToken cancellationToken)
		{
			var result = await _userServices.TogglePublishStatusAsync(id, cancellationToken);

			return result.IsSuccess ? NoContent() : result.ToProblem();
		}

		[HttpPut("{id}/user-unlock")]
		public async Task<IActionResult> UserUnlock([FromRoute] string id, CancellationToken cancellationToken)
		{
			var result = await _userServices.UserUnlock(id, cancellationToken);

			return result.IsSuccess ? NoContent() : result.ToProblem();
		}
	}
}
