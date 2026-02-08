using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicketEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TicketEvents_TicketId",
                table: "TicketEvents");

            migrationBuilder.DropIndex(
                name: "IX_TicketEvents_CreatedAt",
                table: "TicketEvents");

            migrationBuilder.AlterColumn<string>(
                name: "DataJson",
                table: "TicketEvents",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ActorUserId",
                table: "TicketEvents",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_TicketEvents_TicketId_CreatedAt",
                table: "TicketEvents",
                columns: new[] { "TicketId", "CreatedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TicketEvents_TicketId_CreatedAt",
                table: "TicketEvents");

            migrationBuilder.AlterColumn<string>(
                name: "DataJson",
                table: "TicketEvents",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ActorUserId",
                table: "TicketEvents",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketEvents_CreatedAt",
                table: "TicketEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TicketEvents_TicketId",
                table: "TicketEvents",
                column: "TicketId");
        }
    }
}
