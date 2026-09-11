using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Data.Migrations.Social
{
    /// <inheritdoc />
    public partial class addProfileFollowHighlights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bio",
                schema: "app_social",
                table: "user_profile",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "website_url",
                schema: "app_social",
                table: "user_profile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "user_highlights",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cover_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_highlights", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_highlights_files_cover_file_id",
                        column: x => x.cover_file_id,
                        principalSchema: "app_social",
                        principalTable: "files",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_highlights_record_status_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "record_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_highlights_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Backfill: user_name was historically populated from the full name (spaces,
            // diacritics, ...), which isn't safe to use as a @username / profile URL
            // segment. Any row that doesn't already look like a handle falls back to the
            // local part of its email so the unique index below can be created cleanly.
            migrationBuilder.Sql(@"
                UPDATE app_social.users
                SET user_name = split_part(email, '@', 1)
                WHERE user_name !~ '^[A-Za-z0-9._]+$';
            ");

            migrationBuilder.CreateIndex(
                name: "ix_users_user_name",
                schema: "app_social",
                table: "users",
                column: "user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_highlights_cover_file_id",
                schema: "app_social",
                table: "user_highlights",
                column: "cover_file_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_highlights_created_by_user_id",
                schema: "app_social",
                table: "user_highlights",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_highlights_reccord_status_id",
                schema: "app_social",
                table: "user_highlights",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_highlights_user_id",
                schema: "app_social",
                table: "user_highlights",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_highlights",
                schema: "app_social");

            migrationBuilder.DropIndex(
                name: "ix_users_user_name",
                schema: "app_social",
                table: "users");

            migrationBuilder.DropColumn(
                name: "bio",
                schema: "app_social",
                table: "user_profile");

            migrationBuilder.DropColumn(
                name: "website_url",
                schema: "app_social",
                table: "user_profile");
        }
    }
}
