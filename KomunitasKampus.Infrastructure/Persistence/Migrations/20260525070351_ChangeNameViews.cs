using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunitasKampus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_stories_organization_id",
                table: "stories");

            migrationBuilder.RenameIndex(
                name: "ix_story_views_story_id_account_id",
                table: "story_views",
                newName: "ux_story_views_story_account");

            migrationBuilder.AddColumn<DateTime>(
                name: "viewed_at",
                table: "story_views",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())");

            migrationBuilder.CreateIndex(
                name: "ix_stories_expires_at",
                table: "stories",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_stories_org_expired_expires_at",
                table: "stories",
                columns: new[] { "organization_id", "is_expired", "expires_at" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_stories_text_or_media_content",
                table: "stories",
                sql: "(\r\n    media_type = 'Text'\r\n    AND text_content IS NOT NULL\r\n    AND media_url IS NULL\r\n)\r\nOR\r\n(\r\n    media_type IN ('Image', 'Video')\r\n    AND media_url IS NOT NULL\r\n)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_stories_expires_at",
                table: "stories");

            migrationBuilder.DropIndex(
                name: "ix_stories_org_expired_expires_at",
                table: "stories");

            migrationBuilder.DropCheckConstraint(
                name: "ck_stories_text_or_media_content",
                table: "stories");

            migrationBuilder.DropColumn(
                name: "viewed_at",
                table: "story_views");

            migrationBuilder.RenameIndex(
                name: "ux_story_views_story_account",
                table: "story_views",
                newName: "ix_story_views_story_id_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_stories_organization_id",
                table: "stories",
                column: "organization_id");
        }
    }
}
