using Microsoft.AspNetCore.WebUtilities;
using SurveyBasket.Api.Contracts.Users;
using System.Text;

namespace SurveyBasket.Api.Services;

public class UserServices(UserManager<ApplicationUser> userManager, IRoleServices roleServices, ApplicationDbContext db,
	ILogger<UserServices> logger) : IUserServices
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly IRoleServices _roleServices = roleServices;
	private readonly ApplicationDbContext _db = db;
	private readonly ILogger<UserServices> _logger = logger;

	public async Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
		await (from u in _db.Users
			   join ur in _db.UserRoles
			   on u.Id equals ur.UserId
			   join r in _db.Roles
			   on ur.RoleId equals r.Id into roles
			   where !roles.Any(x => x.Name == DefaultRoles.Member.Name)
			   select new
			   {
				   u.Id,
				   u.FirstName,
				   u.LastName,
				   u.Email,
				   u.IsDisabled,
				   Roles = roles.Select(x => x.Name!).ToList()
			   })
				.GroupBy(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.IsDisabled })
				.Select(u => new UserResponse(
					u.Key.Id,
					u.Key.FirstName,
					u.Key.LastName,
					u.Key.Email,
					u.Key.IsDisabled,
					u.SelectMany(x => x.Roles)
					))
				.ToListAsync(cancellationToken);

	public async Task<Result<UserResponse>> GetAsync(string Id)
	{
		if (await _userManager.FindByIdAsync(Id) is not { } user)
			return Result.Failure<UserResponse>(UserErrors.UserNotFound);

		var userRoles = await _userManager.GetRolesAsync(user);

		var response = (user, userRoles).Adapt<UserResponse>();

		return Result.Success(response);
	}


	public async Task<Result<UserResponse>> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
	{
		var userIsExist = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

		if (userIsExist)
			return Result.Failure<UserResponse>(UserErrors.DuplicatedEmail);

		var allowedRules = await _roleServices.GetAllAsync(cancellationToken: cancellationToken);

		if (request.Roles.Except(allowedRules.Select(x => x.Name)).Any())
			return Result.Failure<UserResponse>(UserErrors.InvalidRoles);

		var user = request.Adapt<ApplicationUser>();

		var result = await _userManager.CreateAsync(user);


		if (result.Succeeded)
		{
			var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

			_logger.LogInformation("Confirmation code: {code}", code);

			await _userManager.AddToRolesAsync(user, request.Roles);

			var response = (user, request.Roles).Adapt<UserResponse>();

			return Result.Success(response);

		}

		var error = result.Errors.First();

		return Result.Failure<UserResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}
	public async Task<Result> AddPasswordToUserAsync(AddPasswordToUserRequest request, CancellationToken cancellationToken = default)
	{
		if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
			return Result.Failure(UserErrors.InvalidCode);


		var code = request.Code;

		try
		{
			code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

		}
		catch (FormatException)
		{
			return Result.Failure(UserErrors.InvalidCode);
		}

		var result = await _userManager.ConfirmEmailAsync(user, code); ;

		if (result.Succeeded)
		{
			await _userManager.AddPasswordAsync(user, request.Password);

			return Result.Success();
		}

		var error = result.Errors.First();

		return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}

	public async Task<Result> UpdateAsync(string id, UpdateUserRequest request
	, CancellationToken cancellationToken = default)
	{
		var userIsExist = await _userManager.Users.AnyAsync(x => x.Email == request.Email && x.Id != id
		, cancellationToken);

		if (userIsExist)
			return Result.Failure(UserErrors.DuplicatedEmail);

		var allowedRoles = await _roleServices.GetAllAsync(cancellationToken: cancellationToken);

		if (request.Roles.Except(allowedRoles.Select(x => x.Name)).Any())
			return Result.Failure(UserErrors.InvalidRoles);

		if (await _userManager.FindByIdAsync(id) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		user = request.Adapt(user);

		var result = await _userManager.UpdateAsync(user);

		if (result.Succeeded)
		{
			await _db.UserRoles.
				Where(x => x.UserId == id)
				.ExecuteDeleteAsync(cancellationToken);
			await _userManager.AddToRolesAsync(user, request.Roles);

			return Result.Success();
		}

		var error = result.Errors.First();

		return Result.Failure<UserResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}


	public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
	{
		var user = await _userManager.FindByIdAsync(userId);

		var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

		if (result.Succeeded)
			return Result.Success();

		var error = result.Errors.First();

		return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}

	public async Task<Result<UserProfileResponse>> GetUserProfileAsync(string userId)
	{
		var user = await _userManager.Users
			.Where(x => x.Id == userId).
			ProjectToType<UserProfileResponse>()
			.SingleAsync();

		return Result.Success(user);
	}

	public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
	{
		//var user = await _userManager.FindByIdAsync(userId);

		//user = request.Adapt(user);

		//await _userManager.UpdateAsync(user!);

		await _userManager.Users
			.Where(x => x.Id == userId)
			.ExecuteUpdateAsync(setters =>
			setters
			.SetProperty(x => x.FirstName, request.FirstName)
			.SetProperty(x => x.LastName, request.LastName)
			);
		return Result.Success();
	}

	public async Task<Result> TogglePublishStatusAsync(string id, CancellationToken cancellationToken)
	{
		if (await _userManager.FindByIdAsync(id) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		user.IsDisabled = !user.IsDisabled;

		var result = await _userManager.UpdateAsync(user);

		if (result.Succeeded)
			return Result.Success();

		var error = result.Errors.First();

		return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}

	public async Task<Result> UserUnlock(string id, CancellationToken cancellationToken)
	{
		if (await _userManager.FindByIdAsync(id) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		var result = await _userManager.SetLockoutEndDateAsync(user, null);

		if (result.Succeeded)
			return Result.Success();

		var error = result.Errors.First();

		return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}
}
