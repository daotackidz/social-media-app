using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Data.Migrations.Social
{
    /// <inheritdoc />
    public partial class AddMessageReplyToMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "reply_to_message_id",
                schema: "app_social",
                table: "messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_messages_reply_to_message_id",
                schema: "app_social",
                table: "messages",
                column: "reply_to_message_id");

            migrationBuilder.AddForeignKey(
                name: "fk_messages_messages_reply_to_message_id",
                schema: "app_social",
                table: "messages",
                column: "reply_to_message_id",
                principalSchema: "app_social",
                principalTable: "messages",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_messages_messages_reply_to_message_id",
                schema: "app_social",
                table: "messages");

            migrationBuilder.DropIndex(
                name: "ix_messages_reply_to_message_id",
                schema: "app_social",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "reply_to_message_id",
                schema: "app_social",
                table: "messages");
        }
    }
}
