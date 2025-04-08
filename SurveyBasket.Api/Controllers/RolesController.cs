using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Api.Contracts.Roles;
using SurveyBasket.Api.Services;

namespace SurveyBasket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController(IRoleServices roleServices) : ControllerBase
    {
		private readonly IRoleServices _roleServices = roleServices;

		[HttpGet("")]
        [HasPermission(Permissions.ReadRoles)]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDisabled, CancellationToken cancellationToken)
        {
            var result = await _roleServices.GetAllAsync(includeDisabled, cancellationToken);

            return Ok(result);
        }
		[HttpGet("{id}")]
		[HasPermission(Permissions.ReadRoles)]
		public async Task<IActionResult> Get([FromRoute] string id)
		{
			var result = await _roleServices.GetAsync(id);

			return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
		}
		[HttpPost("")]
		[HasPermission(Permissions.AddRoles)]
		public async Task<IActionResult> Add([FromBody] RoleRequest request)
		{
			var result = await _roleServices.AddAsync(request);

			return result.IsSuccess ? CreatedAtAction(nameof (Get),new { id = result.Value.Id},result.Value.Id) : result.ToProblem();
		}

		[HttpPut("{id}")]
		[HasPermission(Permissions.UpdateRoles)]
		public async Task<IActionResult> Update([FromRoute]string id, [FromBody] RoleRequest request)
		{
			var result = await _roleServices.UpdateAsync(id, request);

			return result.IsSuccess ? NoContent() : result.ToProblem();
		}

		[HttpPut("{id}/toggle-status")]
		[HasPermission(Permissions.UpdateRoles)]
		public async Task<IActionResult> TogglePublish([FromRoute] string id, CancellationToken cancellationToken)
		{
			var result = await _roleServices.ToggleStatusAsync(id, cancellationToken);

			return result.IsSuccess ? NoContent() : result.ToProblem();
		}

	}
}
