using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Data.Migrations.Social
{
    /// <inheritdoc />
    public partial class updateColumnTypeUserPending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "full_name",
                schema: "app_social",
                table: "user_pending_registrations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_of_birth",
                schema: "app_social",
                table: "user_pending_registrations",
                type: "timestamp without time zone",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldMaxLength: 50,
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AddColumn<int>(
                name: "type",
                schema: "app_social",
                table: "user_pending_registrations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                schema: "app_social",
                table: "user_pending_registrations");

            migrationBuilder.AlterColumn<string>(
                name: "full_name",
                schema: "app_social",
                table: "user_pending_registrations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_of_birth",
                schema: "app_social",
                table: "user_pending_registrations",
                type: "timestamp without time zone",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 5);
        }
    }
}
