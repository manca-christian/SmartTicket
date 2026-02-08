using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketCommentAuthorFkAndIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TicketComments_CreatedAt",
                table: "TicketComments");

            migrationBuilder.DropIndex(
                name: "IX_TicketComments_TicketId",
                table: "TicketComments");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_TicketId_CreatedAt",
                table: "TicketComments",
                columns: new[] { "TicketId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComments_Users_AuthorUserId",
                table: "TicketComments",
                column: "AuthorUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketComments_Users_AuthorUserId",
                table: "TicketComments");

            migrationBuilder.DropIndex(
                name: "IX_TicketComments_TicketId_CreatedAt",
                table: "TicketComments");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_CreatedAt",
                table: "TicketComments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_TicketId",
                table: "TicketComments",
                column: "TicketId");
        }
    }
}
