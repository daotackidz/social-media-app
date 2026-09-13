using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Data.Migrations.Social
{
    /// <inheritdoc />
    public partial class ExtendNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "create_datetime",
                schema: "app_social",
                table: "notifications",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "create_datetime_unix",
                schema: "app_social",
                table: "notifications",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                schema: "app_social",
                table: "notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "display_order",
                schema: "app_social",
                table: "notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "note",
                schema: "app_social",
                table: "notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reccord_status_id",
                schema: "app_social",
                table: "notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "story_id",
                schema: "app_social",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_created_by_user_id",
                schema: "app_social",
                table: "notifications",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_reccord_status_id",
                schema: "app_social",
                table: "notifications",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_story_id",
                schema: "app_social",
                table: "notifications",
                column: "story_id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_record_status_reccord_status_id",
                schema: "app_social",
                table: "notifications",
                column: "reccord_status_id",
                principalSchema: "app_social",
                principalTable: "record_status",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_stories_story_id",
                schema: "app_social",
                table: "notifications",
                column: "story_id",
                principalSchema: "app_social",
                principalTable: "stories",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notifications_record_status_reccord_status_id",
                schema: "app_social",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_stories_story_id",
                schema: "app_social",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_created_by_user_id",
                schema: "app_social",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_reccord_status_id",
                schema: "app_social",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_story_id",
                schema: "app_social",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "create_datetime",
                schema: "app_social",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "create_datetime_unix",
                schema: "app_social",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                schema: "app_social",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "display_order",
                schema: "app_social",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "note",
                schema: "app_social",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "reccord_status_id",
                schema: "app_social",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "story_id",
                schema: "app_social",
                table: "notifications");
        }
    }
}
