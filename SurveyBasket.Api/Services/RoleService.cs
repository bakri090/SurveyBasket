using SurveyBasket.Api.Contracts.Roles;

namespace SurveyBasket.Api.Services;

public class RoleService(RoleManager<ApplicationRole> roleManager,ApplicationDbContext db) : IRoleService
{
	private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
	private readonly ApplicationDbContext _db = db;

	public async Task<IEnumerable<RoleResponse>> GetAllAsync( bool? includeDisabled = false,
		CancellationToken cancellationToken = default)
	{
		return await _roleManager.Roles
			.Where(x => !x.IsDefault && (!x.IsDeleted || (includeDisabled.HasValue && includeDisabled.Value)))
			.ProjectToType<RoleResponse>()
			.ToListAsync(cancellationToken: cancellationToken);
	}

	public async Task<Result<RoleDetailResponse>> GetAsync(string id)
	{
		if(await _roleManager.FindByIdAsync(id) is not { } role)
			return Result.Failure<RoleDetailResponse>(RoleErrors.RoleNotFound);

		var permissions = await _roleManager.GetClaimsAsync(role);

		var response = new RoleDetailResponse(role.Id, role.Name!, role.IsDeleted,permissions.Select(x => x.Value));

		return Result.Success(response);
	}
	public async Task<Result<RoleDetailResponse>> AddAsync(RoleRequest request)
	{
		var isExist = await _roleManager.RoleExistsAsync(request.Name);

		if(isExist)
			return Result.Failure<RoleDetailResponse>(RoleErrors.DuplicatedRole);
		
		var allowedPermissions = Permissions.GetAllPermissions();

		if (request.Permissions.Except(allowedPermissions).Any())
			return Result.Failure<RoleDetailResponse>(RoleErrors.InvalidPermissions);

		var role = new ApplicationRole
		{
			Name = request.Name,
			ConcurrencyStamp = Guid.NewGuid().ToString(),
		};

		var result =await _roleManager.CreateAsync(role);

		if(result.Succeeded)
		{
			var permissions = request.Permissions
				.Select(x => new IdentityRoleClaim<string>
				{
					ClaimType = Permissions.Type
				,
					ClaimValue = x
				,
					RoleId = role.Id
				});
			await _db.AddRangeAsync(permissions);
			await _db.SaveChangesAsync();

			var response = new RoleDetailResponse(role.Id, role.Name,role.IsDeleted,request.Permissions);

			return Result.Success(response);
		}
		var error = result.Errors.First();

		return Result.Failure<RoleDetailResponse>(new Error(error.Code,error.Description,StatusCodes.Status400BadRequest));
	}

	public async Task<Result> UpdateAsync(string id,RoleRequest request)
	{
		var isExist = await _roleManager.Roles.AnyAsync(x => x.Name == request.Name && x.Id != id);
		if (isExist)
			return Result.Failure<RoleDetailResponse>(RoleErrors.DuplicatedRole);

		if(await _roleManager.FindByIdAsync(id) is not { } role)
			return Result.Failure<RoleDetailResponse>(RoleErrors.RoleNotFound);

		role.Name = request.Name;

		var result = await _roleManager.UpdateAsync(role);

		if (result.Succeeded)
		{
			var currentPermissions = await _db.RoleClaims.Where(x => x.RoleId == id && x.ClaimType == Permissions.Type)
				.Select(x => x.ClaimValue).ToListAsync();

			var newPermissions = request.Permissions.Except(currentPermissions)
				.Select(x => new IdentityRoleClaim<string>
				{
					ClaimType = Permissions.Type,
					ClaimValue = x,
					RoleId = role.Id
				});
			var removedPermissions = currentPermissions.Except(request.Permissions);
			
			await _db.RoleClaims.Where(x => x.RoleId == id && removedPermissions.Contains(x.ClaimValue))
				.ExecuteDeleteAsync();

			await _db.AddRangeAsync(newPermissions);
			await _db.SaveChangesAsync();

			return Result.Success();
		}
		var error = result.Errors.First();

		return Result.Failure<RoleDetailResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}

	public async Task<Result> ToggleStatusAsync(string id, CancellationToken cancellationToken = default)
	{
		if(await _roleManager.FindByIdAsync(id) is not { } role)
			return Result.Failure<RoleDetailResponse>(RoleErrors.RoleNotFound);

		role.IsDeleted = !role.IsDeleted;

		await _roleManager.UpdateAsync(role);

		return Result.Success();
	}
}
