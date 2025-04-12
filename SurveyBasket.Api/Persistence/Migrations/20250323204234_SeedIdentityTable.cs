using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SurveyBasket.Api.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class SeedIdentityTable : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.InsertData(
				table: "AspNetRoles",
				columns: new[] { "Id", "ConcurrencyStamp", "IsDefault", "IsDeleted", "Name", "NormalizedName" },
				values: new object[,]
				{
					{ "904f2abe-cfe1-45e6-be3c-66f4a5e2241e", "a17d788e-e065-4f43-87c2-0c97a3b6788b", true, false, "Member", "MEMBER" },
					{ "94a21f37-5304-424d-83f2-325e1a782334", "8775b39f-4190-47f1-b7a8-1a9d41ae8d5a", false, false, "Admin", "ADMIN" }
				});

			migrationBuilder.InsertData(
				table: "AspNetUsers",
				columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
				values: new object[] { "6269b8b9-29c7-49e9-86ec-2587b0814197", 0, "32fcebc6-5b1c-45cd-8ceb-47c7634f4c57", "admin@survey-basket.com", true, "Survey Basket", "Admin", false, null, "ADMIN@SURVEY-BASKET.COM", "ADMIN@SURVEY-BASKET.COM", "AQAAAAIAAYagAAAAEPdsijkJMOJH4OujRMhKI6qqgLSCS/KIkBia0lsqd+DCPONbO0qdUKSrOgo4f5rqbA==", null, false, "F878C59C-D4F9-4A26-A5AA-08277907BFA3", false, "admin@survey-basket.com" });

			migrationBuilder.InsertData(
				table: "AspNetRoleClaims",
				columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
				values: new object[,]
				{
					{ 1, "permissions", "polls:read", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 2, "permissions", "polls:add", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 3, "permissions", "polls:update", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 4, "permissions", "polls:Delete", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 5, "permissions", "questions:Read", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 6, "permissions", "questions:Add", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 7, "permissions", "questions:update", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 8, "permissions", "users:read", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 9, "permissions", "users:add", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 10, "permissions", "users:update", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 11, "permissions", "roles:read", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 12, "permissions", "roles:add", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 13, "permissions", "roles:update", "94a21f37-5304-424d-83f2-325e1a782334" },
					{ 14, "permissions", "results:read", "94a21f37-5304-424d-83f2-325e1a782334" }
				});

			migrationBuilder.InsertData(
				table: "AspNetUserRoles",
				columns: new[] { "RoleId", "UserId" },
				values: new object[] { "94a21f37-5304-424d-83f2-325e1a782334", "6269b8b9-29c7-49e9-86ec-2587b0814197" });
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 1);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 2);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 3);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 4);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 5);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 6);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 7);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 8);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 9);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 10);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 11);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 12);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 13);

			migrationBuilder.DeleteData(
				table: "AspNetRoleClaims",
				keyColumn: "Id",
				keyValue: 14);

			migrationBuilder.DeleteData(
				table: "AspNetRoles",
				keyColumn: "Id",
				keyValue: "904f2abe-cfe1-45e6-be3c-66f4a5e2241e");

			migrationBuilder.DeleteData(
				table: "AspNetUserRoles",
				keyColumns: new[] { "RoleId", "UserId" },
				keyValues: new object[] { "94a21f37-5304-424d-83f2-325e1a782334", "6269b8b9-29c7-49e9-86ec-2587b0814197" });

			migrationBuilder.DeleteData(
				table: "AspNetRoles",
				keyColumn: "Id",
				keyValue: "94a21f37-5304-424d-83f2-325e1a782334");

			migrationBuilder.DeleteData(
				table: "AspNetUsers",
				keyColumn: "Id",
				keyValue: "6269b8b9-29c7-49e9-86ec-2587b0814197");
		}
	}
}
