using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunitasKampus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_memberships_accounts_account_id",
                table: "memberships");

            migrationBuilder.DropForeignKey(
                name: "fk_memberships_organizations_organization_id",
                table: "memberships");

            migrationBuilder.DropIndex(
                name: "ix_memberships_account_id_organization_id",
                table: "memberships");

            migrationBuilder.DropIndex(
                name: "ix_memberships_organization_id",
                table: "memberships");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "memberships",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "invite_type",
                table: "memberships",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_accounts_actor_id",
                        column: x => x.actor_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_notifications_accounts_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_memberships_org_status_invite_type",
                table: "memberships",
                columns: new[] { "organization_id", "status", "invite_type" });

            migrationBuilder.CreateIndex(
                name: "ux_memberships_account_org_pending_accepted",
                table: "memberships",
                columns: new[] { "account_id", "organization_id" },
                unique: true,
                filter: "\"deleted_at\" IS NULL AND \"status\" IN ('Pending', 'Accepted')");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_actor_id",
                table: "notifications",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_recipient_read_created",
                table: "notifications",
                columns: new[] { "recipient_id", "is_read", "created_at" });

            migrationBuilder.AddForeignKey(
                name: "fk_memberships_accounts_account_id",
                table: "memberships",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_memberships_organizations_organization_id",
                table: "memberships",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_memberships_accounts_account_id",
                table: "memberships");

            migrationBuilder.DropForeignKey(
                name: "fk_memberships_organizations_organization_id",
                table: "memberships");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_memberships_org_status_invite_type",
                table: "memberships");

            migrationBuilder.DropIndex(
                name: "ux_memberships_account_org_pending_accepted",
                table: "memberships");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "memberships",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "invite_type",
                table: "memberships",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.CreateIndex(
                name: "ix_memberships_account_id_organization_id",
                table: "memberships",
                columns: new[] { "account_id", "organization_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_memberships_organization_id",
                table: "memberships",
                column: "organization_id");

            migrationBuilder.AddForeignKey(
                name: "fk_memberships_accounts_account_id",
                table: "memberships",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_memberships_organizations_organization_id",
                table: "memberships",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
