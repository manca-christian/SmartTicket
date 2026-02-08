using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CreatedByUserId_CreatedAt",
                table: "Tickets",
                columns: new[] { "CreatedByUserId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedToUserId_AssignedAt",
                table: "Tickets",
                columns: new[] { "AssignedToUserId", "AssignedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "UserId", "ExpiresAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_CreatedByUserId_CreatedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AssignedToUserId_AssignedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId_ExpiresAt",
                table: "RefreshTokens");
        }
    }
}
