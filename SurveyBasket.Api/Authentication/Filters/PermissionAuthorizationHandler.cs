namespace SurveyBasket.Api.Authentication.Filters;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
	{
		// [نفس الشي بطريقه اسهل [1
		//var user = context.User.Identity;
		//if (!user.IsAuthenticated || user is null)
		//	return;
		//var hasPermission = context.User.Claims.Any(x => x.Value == requirement.Permission && x.Type == Permissions.Type);
		//if (!hasPermission) 
		//	return;

		// [2]
		if (context.User.Identity is not { IsAuthenticated: true } ||
			!context.User.Claims.Any(x => x.Value == requirement.Permission && x.Type == Permissions.Type))
			return;

		context.Succeed(requirement);
		return;
	}
}
