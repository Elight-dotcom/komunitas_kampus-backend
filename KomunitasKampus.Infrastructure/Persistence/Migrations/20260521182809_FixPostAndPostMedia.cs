using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunitasKampus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPostAndPostMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_posts_organization_id",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_post_media_post_id",
                table: "post_media");

            migrationBuilder.CreateIndex(
                name: "ix_posts_organization_id_is_pinned_pin_order_created_at",
                table: "posts",
                columns: new[] { "organization_id", "is_pinned", "pin_order", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_post_media_post_id_order_index",
                table: "post_media",
                columns: new[] { "post_id", "order_index" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_posts_organization_id_is_pinned_pin_order_created_at",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_post_media_post_id_order_index",
                table: "post_media");

            migrationBuilder.CreateIndex(
                name: "ix_posts_organization_id",
                table: "posts",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_media_post_id",
                table: "post_media",
                column: "post_id");
        }
    }
}
