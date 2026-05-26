using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunitasKampus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInteractionsInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_comments_users_user_id",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "ix_shares_post_id_user_id",
                table: "shares");

            migrationBuilder.DropIndex(
                name: "ix_shares_user_id",
                table: "shares");

            migrationBuilder.DropIndex(
                name: "ix_likes_post_id_user_id",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "ix_likes_user_id",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "ix_comments_post_id",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "ix_comments_user_id",
                table: "comments");

            migrationBuilder.AddColumn<string>(
                name: "platform",
                table: "shares",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id1",
                table: "shares",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id1",
                table: "likes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "deleted_reason",
                table: "comments",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "account_id",
                table: "comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id1",
                table: "comments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_shares_post_created",
                table: "shares",
                columns: new[] { "post_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_shares_user_id1",
                table: "shares",
                column: "user_id1");

            migrationBuilder.CreateIndex(
                name: "ix_shares_user_post",
                table: "shares",
                columns: new[] { "user_id", "post_id" });

            migrationBuilder.CreateIndex(
                name: "ix_likes_post_id",
                table: "likes",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_likes_user_id1",
                table: "likes",
                column: "user_id1");

            migrationBuilder.CreateIndex(
                name: "ux_likes_user_post",
                table: "likes",
                columns: new[] { "user_id", "post_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_comments_account_id",
                table: "comments",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_post_created",
                table: "comments",
                columns: new[] { "post_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_comments_user_id1",
                table: "comments",
                column: "user_id1");

            migrationBuilder.CreateIndex(
                name: "ix_comments_user_post_created",
                table: "comments",
                columns: new[] { "user_id", "post_id", "created_at" });

            migrationBuilder.AddForeignKey(
                name: "fk_comments_accounts_account_id",
                table: "comments",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_comments_accounts_user_id",
                table: "comments",
                column: "user_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_comments_users_user_id1",
                table: "comments",
                column: "user_id1",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_likes_users_user_id1",
                table: "likes",
                column: "user_id1",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_shares_users_user_id1",
                table: "shares",
                column: "user_id1",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_comments_accounts_account_id",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "fk_comments_accounts_user_id",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "fk_comments_users_user_id1",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "fk_likes_users_user_id1",
                table: "likes");

            migrationBuilder.DropForeignKey(
                name: "fk_shares_users_user_id1",
                table: "shares");

            migrationBuilder.DropIndex(
                name: "ix_shares_post_created",
                table: "shares");

            migrationBuilder.DropIndex(
                name: "ix_shares_user_id1",
                table: "shares");

            migrationBuilder.DropIndex(
                name: "ix_shares_user_post",
                table: "shares");

            migrationBuilder.DropIndex(
                name: "ix_likes_post_id",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "ix_likes_user_id1",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "ux_likes_user_post",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "ix_comments_account_id",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "ix_comments_post_created",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "ix_comments_user_id1",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "ix_comments_user_post_created",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "platform",
                table: "shares");

            migrationBuilder.DropColumn(
                name: "user_id1",
                table: "shares");

            migrationBuilder.DropColumn(
                name: "user_id1",
                table: "likes");

            migrationBuilder.DropColumn(
                name: "account_id",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "user_id1",
                table: "comments");

            migrationBuilder.AlterColumn<string>(
                name: "deleted_reason",
                table: "comments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_shares_post_id_user_id",
                table: "shares",
                columns: new[] { "post_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shares_user_id",
                table: "shares",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_likes_post_id_user_id",
                table: "likes",
                columns: new[] { "post_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_likes_user_id",
                table: "likes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_post_id",
                table: "comments",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_user_id",
                table: "comments",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_comments_users_user_id",
                table: "comments",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
