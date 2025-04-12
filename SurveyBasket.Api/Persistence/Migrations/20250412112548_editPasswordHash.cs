using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyBasket.Api.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class editPasswordHash : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.UpdateData(
				table: "AspNetUsers",
				keyColumn: "Id",
				keyValue: "6269b8b9-29c7-49e9-86ec-2587b0814197",
				column: "PasswordHash",
				value: "AQAAAAIAAYagAAAAEPdsijkJMOJH4OujRMhKI6qqgLSCS/KIkBia0lsqd+DCPONbO0qdUKSrOgo4f5rqbA==");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.UpdateData(
				table: "AspNetUsers",
				keyColumn: "Id",
				keyValue: "6269b8b9-29c7-49e9-86ec-2587b0814197",
				column: "PasswordHash",
				value: "AQAAAAIAAYagAAAAEBoyMHPhrmyFZ2gjFM0MW9i2emkbigWLQicaFDJ0n9cugZhALUW3zb7nJBRBDxMNvA==");
		}
	}
}
