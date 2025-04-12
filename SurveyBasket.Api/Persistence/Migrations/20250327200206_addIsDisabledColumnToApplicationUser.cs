using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyBasket.Api.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class addIsDisabledColumnToApplicationUser : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
				name: "IsDisabled",
				table: "AspNetUsers",
				type: "bit",
				nullable: false,
				defaultValue: false);

		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "IsDisabled",
				table: "AspNetUsers");
		}
	}
}
