using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Data.Migrations.Social
{
    /// <inheritdoc />
    public partial class AddUserProfileIsPrivate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_private",
                schema: "app_social",
                table: "user_profile",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_private",
                schema: "app_social",
                table: "user_profile");
        }
    }
}
