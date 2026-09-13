using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Data.Migrations.Social
{
    /// <inheritdoc />
    public partial class AddUserSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_sessions",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_type = table.Column<int>(type: "integer", nullable: false),
                    session_token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_login_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_sessions_record_status_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "record_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_sessions_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_created_by_user_id",
                schema: "app_social",
                table: "user_sessions",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_reccord_status_id",
                schema: "app_social",
                table: "user_sessions",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_user_id_client_type",
                schema: "app_social",
                table: "user_sessions",
                columns: new[] { "user_id", "client_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_sessions",
                schema: "app_social");
        }
    }
}
