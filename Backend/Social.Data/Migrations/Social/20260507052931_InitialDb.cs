using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Data.Migrations.Social
{
    /// <inheritdoc />
    public partial class InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "app_social");

            migrationBuilder.CreateTable(
                name: "recordstatuses",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    status_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recordstatuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "files",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_name_origin = table.Column<string>(type: "text", nullable: false),
                    file_extension = table.Column<string>(type: "text", nullable: false),
                    file_type = table.Column<int>(type: "integer", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    storage_path = table.Column<string>(type: "text", nullable: false),
                    storage_type = table.Column<int>(type: "integer", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    file_version = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_files", x => x.id);
                    table.ForeignKey(
                        name: "fk_files_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hash_tags",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hash_tag_name = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hash_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_hash_tags_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    salt_password = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    user_locked = table.Column<bool>(type: "boolean", nullable: false),
                    phone_number_comfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    email_comfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    hash_text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    key_reset_password = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    key_reset_password_expiration_date_unix = table.Column<long>(type: "bigint", nullable: true),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    token_expiration_date_unix = table.Column<long>(type: "bigint", nullable: true),
                    user_type = table.Column<int>(type: "integer", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    otp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    otp_expiration_date_unix = table.Column<long>(type: "bigint", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "file_version",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_name_origin = table.Column<string>(type: "text", nullable: false),
                    file_extension = table.Column<string>(type: "text", nullable: false),
                    file_type = table.Column<int>(type: "integer", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    storage_path = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_version", x => x.id);
                    table.ForeignKey(
                        name: "fk_file_version_files_file_id",
                        column: x => x.file_id,
                        principalSchema: "app_social",
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_file_version_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    caption = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    privacy = table.Column<int>(type: "integer", nullable: false),
                    like_count = table.Column<int>(type: "integer", nullable: false),
                    comment_count = table.Column<int>(type: "integer", nullable: false),
                    shared_count = table.Column<int>(type: "integer", nullable: false),
                    is_edit = table.Column<bool>(type: "boolean", nullable: false),
                    is_delete = table.Column<bool>(type: "boolean", nullable: false),
                    original_post_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posts", x => x.id);
                    table.ForeignKey(
                        name: "fk_posts_posts_original_post_id",
                        column: x => x.original_post_id,
                        principalSchema: "app_social",
                        principalTable: "posts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_posts_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_posts_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_files",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_type = table.Column<int>(type: "integer", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_files", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_files_files_file_id",
                        column: x => x.file_id,
                        principalSchema: "app_social",
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_files_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_files_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_profile",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp without time zone", maxLength: 50, nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: false),
                    address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_profile", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_profile_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_profile_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_relations",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    follower_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    following_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relation_type = table.Column<int>(type: "integer", nullable: false),
                    user_relation_status = table.Column<int>(type: "integer", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_relations", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_relations_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_relations_users_follower_user_id",
                        column: x => x.follower_user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_relations_users_following_user_id",
                        column: x => x.following_user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_comments",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    path = table.Column<string>(type: "text", nullable: false),
                    depth = table.Column<int>(type: "integer", nullable: false),
                    like_count = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_comments_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "app_social",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_comments_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_comments_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_files",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_type = table.Column<int>(type: "integer", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_files", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_files_files_file_id",
                        column: x => x.file_id,
                        principalSchema: "app_social",
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_files_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "app_social",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_files_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_hash_tags",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hash_tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_hash_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_hash_tags_hash_tags_hash_tag_id",
                        column: x => x.hash_tag_id,
                        principalSchema: "app_social",
                        principalTable: "hash_tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_hash_tags_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "app_social",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_hash_tags_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_likes",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_likes", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_likes_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "app_social",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_likes_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_likes_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_saves",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_saves", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_saves_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "app_social",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_saves_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_saves_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_tags",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    positionx = table.Column<float>(type: "real", nullable: true),
                    positiony = table.Column<float>(type: "real", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_datetime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_datetime_unix = table.Column<long>(type: "bigint", nullable: false),
                    reccord_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_tags_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "app_social",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_tags_recordstatuses_reccord_status_id",
                        column: x => x.reccord_status_id,
                        principalSchema: "app_social",
                        principalTable: "recordstatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_tags_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "app_social",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_type = table.Column<int>(type: "integer", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: true),
                    post_comment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data = table.Column<string>(type: "text", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_post_comments_post_comment_id",
                        column: x => x.post_comment_id,
                        principalSchema: "app_social",
                        principalTable: "post_comments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notifications_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "app_social",
                        principalTable: "posts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notifications_users_actor_user_id",
                        column: x => x.actor_user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app_social",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_file_version_created_by_user_id",
                schema: "app_social",
                table: "file_version",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_file_version_file_id",
                schema: "app_social",
                table: "file_version",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "ix_file_version_reccord_status_id",
                schema: "app_social",
                table: "file_version",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_files_created_by_user_id",
                schema: "app_social",
                table: "files",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_files_reccord_status_id",
                schema: "app_social",
                table: "files",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_hash_tags_created_by_user_id",
                schema: "app_social",
                table: "hash_tags",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_hash_tags_reccord_status_id",
                schema: "app_social",
                table: "hash_tags",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_actor_user_id",
                schema: "app_social",
                table: "notifications",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_post_comment_id",
                schema: "app_social",
                table: "notifications",
                column: "post_comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_post_id",
                schema: "app_social",
                table: "notifications",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_is_read",
                schema: "app_social",
                table: "notifications",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "ix_post_comments_created_by_user_id",
                schema: "app_social",
                table: "post_comments",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_comments_post_id",
                schema: "app_social",
                table: "post_comments",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_comments_reccord_status_id",
                schema: "app_social",
                table: "post_comments",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_comments_user_id",
                schema: "app_social",
                table: "post_comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_files_created_by_user_id",
                schema: "app_social",
                table: "post_files",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_files_file_id",
                schema: "app_social",
                table: "post_files",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_files_post_id",
                schema: "app_social",
                table: "post_files",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_files_reccord_status_id",
                schema: "app_social",
                table: "post_files",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_hash_tags_created_by_user_id",
                schema: "app_social",
                table: "post_hash_tags",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_hash_tags_hash_tag_id",
                schema: "app_social",
                table: "post_hash_tags",
                column: "hash_tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_hash_tags_post_id",
                schema: "app_social",
                table: "post_hash_tags",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_hash_tags_reccord_status_id",
                schema: "app_social",
                table: "post_hash_tags",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_likes_created_by_user_id",
                schema: "app_social",
                table: "post_likes",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_likes_post_id",
                schema: "app_social",
                table: "post_likes",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_likes_reccord_status_id",
                schema: "app_social",
                table: "post_likes",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_likes_user_id",
                schema: "app_social",
                table: "post_likes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_saves_created_by_user_id",
                schema: "app_social",
                table: "post_saves",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_saves_post_id",
                schema: "app_social",
                table: "post_saves",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_saves_reccord_status_id",
                schema: "app_social",
                table: "post_saves",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_saves_user_id",
                schema: "app_social",
                table: "post_saves",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_tags_created_by_user_id",
                schema: "app_social",
                table: "post_tags",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_tags_post_id",
                schema: "app_social",
                table: "post_tags",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_tags_reccord_status_id",
                schema: "app_social",
                table: "post_tags",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_tags_user_id",
                schema: "app_social",
                table: "post_tags",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_created_by_user_id",
                schema: "app_social",
                table: "posts",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_original_post_id",
                schema: "app_social",
                table: "posts",
                column: "original_post_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_reccord_status_id",
                schema: "app_social",
                table: "posts",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_user_id",
                schema: "app_social",
                table: "posts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_recordstatuses_created_by_user_id",
                schema: "app_social",
                table: "recordstatuses",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_files_created_by_user_id",
                schema: "app_social",
                table: "user_files",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_files_file_id",
                schema: "app_social",
                table: "user_files",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_files_reccord_status_id",
                schema: "app_social",
                table: "user_files",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_files_user_id",
                schema: "app_social",
                table: "user_files",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_profile_created_by_user_id",
                schema: "app_social",
                table: "user_profile",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_profile_reccord_status_id",
                schema: "app_social",
                table: "user_profile",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_profile_user_id",
                schema: "app_social",
                table: "user_profile",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_relations_created_by_user_id",
                schema: "app_social",
                table: "user_relations",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_relations_follower_user_id",
                schema: "app_social",
                table: "user_relations",
                column: "follower_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_relations_following_user_id",
                schema: "app_social",
                table: "user_relations",
                column: "following_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_relations_reccord_status_id",
                schema: "app_social",
                table: "user_relations",
                column: "reccord_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_created_by_user_id",
                schema: "app_social",
                table: "users",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "app_social",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_users_hash_text_user_name_password",
                schema: "app_social",
                table: "users",
                columns: new[] { "hash_text", "user_name", "password" });

            migrationBuilder.CreateIndex(
                name: "ix_users_phone_number",
                schema: "app_social",
                table: "users",
                column: "phone_number");

            migrationBuilder.CreateIndex(
                name: "ix_users_reccord_status_id",
                schema: "app_social",
                table: "users",
                column: "reccord_status_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "file_version",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "post_files",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "post_hash_tags",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "post_likes",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "post_saves",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "post_tags",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "user_files",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "user_profile",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "user_relations",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "post_comments",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "hash_tags",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "files",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "posts",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "users",
                schema: "app_social");

            migrationBuilder.DropTable(
                name: "recordstatuses",
                schema: "app_social");
        }
    }
}
