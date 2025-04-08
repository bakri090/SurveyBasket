namespace SurveyBasket.Api.Abstractions.Consts;

public static class Permissions
{
	public static string Type { get; } = "permissions";

	public const string ReadPolls = "polls:read";
	public const string AddPolls = "polls:add";
	public const string UpdatePolls = "polls:update";
	public const string DeletePolls = "polls:Delete";

	public const string ReadQuestions = "questions:Read";
	public const string AddQuestions = "questions:Add";
	public const string UpdateQuestions = "questions:update";

	public const string ReadUsers = "users:read";
	public const string AddUsers = "users:add";
	public const string UpdateUsers = "users:update";

	public const string ReadRoles = "roles:read";
	public const string AddRoles = "roles:add";
	public const string UpdateRoles = "roles:update";

	public const string ReadResults = "results:read";

	public static IList<string?> GetAllPermissions () =>
		typeof (Permissions).GetFields().Select(x => x.GetValue(x) as string).ToList();
}
