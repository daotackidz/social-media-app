using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Data.Migrations.Social
{
    /// <inheritdoc />
    public partial class AddMessaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conversations",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_group = table.Column<bool>(type: "boolean", nullable: false),
                    title = table.Column<string>(type: "text", nullable: true),
                    last_message_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conversations", x => x.id);
                    table.ForeignKey(
                        name: "fk_conversations_record_status_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "record_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conversation_participants",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_read_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conversation_participants", x => x.id);
                    table.ForeignKey(
                        name: "fk_conversation_participants_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalSchema: "app_social",
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_conversation_participants_record_status_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "record_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_conversation_participants_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    message_type = table.Column<int>(type: "integer", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_messages_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalSchema: "app_social",
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_messages_files_file_id",
                        column: x => x.file_id,
                        principalSchema: "app_social",
                        principalTable: "files",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_messages_record_status_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "record_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_messages_users_sender_user_id",
                        column: x => x.sender_user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_conversation_participants_conversation_id",
                schema: "app_social",
                table: "conversation_participants",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversation_participants_created_by_user_id",
                schema: "app_social",
                table: "conversation_participants",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversation_participants_reccord_status_id",
                schema: "app_social",
                table: "conversation_participants",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversation_participants_user_id",
                schema: "app_social",
                table: "conversation_participants",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversations_created_by_user_id",
                schema: "app_social",
                table: "conversations",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversations_reccord_status_id",
                schema: "app_social",
                table: "conversations",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_conversation_id",
                schema: "app_social",
                table: "messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_created_by_user_id",
                schema: "app_social",
                table: "messages",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_file_id",
                schema: "app_social",
                table: "messages",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_reccord_status_id",
                schema: "app_social",
                table: "messages",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_sender_user_id",
                schema: "app_social",
                table: "messages",
                column: "sender_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "conversation_participants",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "conversations",
                schema: "app_social");
        }
    }
}
