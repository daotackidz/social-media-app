using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Data.Migrations.Social
{
    /// <inheritdoc />
    public partial class AddPostCommentLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "post_comment_likes",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("pk_post_comment_likes", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_comment_likes_post_comments_comment_id",
                        column: x => x.comment_id,
                        principalSchema: "app_social",
                        principalTable: "post_comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_comment_likes_record_status_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "record_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_comment_likes_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_post_comment_likes_comment_id",
                schema: "app_social",
                table: "post_comment_likes",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_comment_likes_created_by_user_id",
                schema: "app_social",
                table: "post_comment_likes",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_comment_likes_reccord_status_id",
                schema: "app_social",
                table: "post_comment_likes",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_comment_likes_user_id",
                schema: "app_social",
                table: "post_comment_likes",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_comment_likes",
                schema: "app_social");
        }
    }
}
