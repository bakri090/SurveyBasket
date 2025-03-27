namespace SurveyBasket.Api.Errors;

public class RoleErrors
{
	public static readonly Error RoleNotFound =
		new("Role.NotFound", "Role is not found", StatusCode: StatusCodes.Status404NotFound);
	
	public static readonly Error DuplicatedRole =
		new("Role.DuplicatedRole", "Another role with the same name is already exists", StatusCode: StatusCodes.Status409Conflict);

	public static readonly Error InvalidPermissions =
		new("Role.InvalidPermissions", "Invalid Permissions",StatusCode: StatusCodes.Status400BadRequest);
}
