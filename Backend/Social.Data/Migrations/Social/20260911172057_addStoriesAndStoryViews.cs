using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Data.Migrations.Social
{
    /// <inheritdoc />
    public partial class addStoriesAndStoryViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stories",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stories", x => x.id);
                    table.ForeignKey(
                        name: "fk_stories_files_file_id",
                        column: x => x.file_id,
                        principalSchema: "app_social",
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_stories_record_status_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "record_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_stories_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "story_views",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    story_id = table.Column<Guid>(type: "uuid", nullable: false),
                    viewer_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_story_views", x => x.id);
                    table.ForeignKey(
                        name: "fk_story_views_record_status_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "record_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_story_views_stories_story_id",
                        column: x => x.story_id,
                        principalSchema: "app_social",
                        principalTable: "stories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_story_views_users_viewer_user_id",
                        column: x => x.viewer_user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stories_created_by_user_id",
                schema: "app_social",
                table: "stories",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_stories_file_id",
                schema: "app_social",
                table: "stories",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "ix_stories_reccord_status_id",
                schema: "app_social",
                table: "stories",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_stories_user_id",
                schema: "app_social",
                table: "stories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_story_views_created_by_user_id",
                schema: "app_social",
                table: "story_views",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_story_views_reccord_status_id",
                schema: "app_social",
                table: "story_views",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_story_views_story_id",
                schema: "app_social",
                table: "story_views",
                column: "story_id");

            migrationBuilder.CreateIndex(
                name: "ix_story_views_viewer_user_id",
                schema: "app_social",
                table: "story_views",
                column: "viewer_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "story_views",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "stories",
                schema: "app_social");
        }
    }
}
