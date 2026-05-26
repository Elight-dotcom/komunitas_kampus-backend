using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunitasKampus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFKLike : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_comments_users_user_id1",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "fk_likes_users_user_id",
                table: "likes");

            migrationBuilder.DropForeignKey(
                name: "fk_likes_users_user_id1",
                table: "likes");

            migrationBuilder.DropForeignKey(
                name: "fk_shares_users_user_id",
                table: "shares");

            migrationBuilder.DropForeignKey(
                name: "fk_shares_users_user_id1",
                table: "shares");

            migrationBuilder.DropIndex(
                name: "ix_shares_user_id1",
                table: "shares");

            migrationBuilder.DropIndex(
                name: "ix_likes_user_id1",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "ix_comments_user_id1",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "user_id1",
                table: "shares");

            migrationBuilder.DropColumn(
                name: "user_id1",
                table: "likes");

            migrationBuilder.DropColumn(
                name: "user_id1",
                table: "comments");

            migrationBuilder.AddForeignKey(
                name: "fk_likes_accounts_user_id",
                table: "likes",
                column: "user_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_shares_accounts_user_id",
                table: "shares",
                column: "user_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_likes_accounts_user_id",
                table: "likes");

            migrationBuilder.DropForeignKey(
                name: "fk_shares_accounts_user_id",
                table: "shares");

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

            migrationBuilder.AddColumn<Guid>(
                name: "user_id1",
                table: "comments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_shares_user_id1",
                table: "shares",
                column: "user_id1");

            migrationBuilder.CreateIndex(
                name: "ix_likes_user_id1",
                table: "likes",
                column: "user_id1");

            migrationBuilder.CreateIndex(
                name: "ix_comments_user_id1",
                table: "comments",
                column: "user_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_comments_users_user_id1",
                table: "comments",
                column: "user_id1",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_likes_users_user_id",
                table: "likes",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_likes_users_user_id1",
                table: "likes",
                column: "user_id1",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_shares_users_user_id",
                table: "shares",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_shares_users_user_id1",
                table: "shares",
                column: "user_id1",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}
