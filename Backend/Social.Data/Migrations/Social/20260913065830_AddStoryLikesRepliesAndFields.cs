using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Data.Migrations.Social
{
    /// <inheritdoc />
    public partial class AddStoryLikesRepliesAndFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "caption",
                schema: "app_social",
                table: "stories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_ai_generated",
                schema: "app_social",
                table: "stories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "like_count",
                schema: "app_social",
                table: "stories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "view_count",
                schema: "app_social",
                table: "stories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "story_likes",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    story_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_story_likes", x => x.id);
                    table.ForeignKey(
                        name: "fk_story_likes_record_status_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "record_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_story_likes_stories_story_id",
                        column: x => x.story_id,
                        principalSchema: "app_social",
                        principalTable: "stories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_story_likes_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "story_replies",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    story_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_story_replies", x => x.id);
                    table.ForeignKey(
                        name: "fk_story_replies_record_status_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "record_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_story_replies_stories_story_id",
                        column: x => x.story_id,
                        principalSchema: "app_social",
                        principalTable: "stories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_story_replies_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_story_likes_created_by_user_id",
                schema: "app_social",
                table: "story_likes",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_story_likes_reccord_status_id",
                schema: "app_social",
                table: "story_likes",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_story_likes_story_id",
                schema: "app_social",
                table: "story_likes",
                column: "story_id");

            migrationBuilder.CreateIndex(
                name: "ix_story_likes_user_id",
                schema: "app_social",
                table: "story_likes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_story_replies_created_by_user_id",
                schema: "app_social",
                table: "story_replies",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_story_replies_reccord_status_id",
                schema: "app_social",
                table: "story_replies",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_story_replies_story_id",
                schema: "app_social",
                table: "story_replies",
                column: "story_id");

            migrationBuilder.CreateIndex(
                name: "ix_story_replies_user_id",
                schema: "app_social",
                table: "story_replies",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "story_likes",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "story_replies",
                schema: "app_social");

            migrationBuilder.DropColumn(
                name: "caption",
                schema: "app_social",
                table: "stories");

            migrationBuilder.DropColumn(
                name: "is_ai_generated",
                schema: "app_social",
                table: "stories");

            migrationBuilder.DropColumn(
                name: "like_count",
                schema: "app_social",
                table: "stories");

            migrationBuilder.DropColumn(
                name: "view_count",
                schema: "app_social",
                table: "stories");
        }
    }
}
