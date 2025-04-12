namespace SurveyBasket.Api.Abstractions.Consts;

public static class DefaultRoles
{
	public partial class Admin
	{

		public const string Name = nameof(Admin);

		public const string RoleId = "94a21f37-5304-424d-83f2-325e1a782334";

		public const string RoleConcurrencyStamp = "8775b39f-4190-47f1-b7a8-1a9d41ae8d5a";
	}

	public partial class Member
	{
		public const string Name = nameof(Member);
		public const string RoleId = "904f2abe-cfe1-45e6-be3c-66f4a5e2241e";
		public const string RoleConcurrencyStamp = "a17d788e-e065-4f43-87c2-0c97a3b6788b";

	}
}
