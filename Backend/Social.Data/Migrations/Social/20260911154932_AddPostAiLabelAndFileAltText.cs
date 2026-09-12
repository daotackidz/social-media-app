using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Data.Migrations.Social
{
    /// <inheritdoc />
    public partial class AddPostAiLabelAndFileAltText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_ai_generated",
                schema: "app_social",
                table: "posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "alt_text",
                schema: "app_social",
                table: "post_files",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_ai_generated",
                schema: "app_social",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "alt_text",
                schema: "app_social",
                table: "post_files");
        }
    }
}
