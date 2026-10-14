using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncBridge.Api.Migrations
{
    /// <inheritdoc />
    public partial class LinkOrderSyncAttemptToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OrderSyncAttempts_OrderId",
                table: "OrderSyncAttempts",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderSyncAttempts_Orders_OrderId",
                table: "OrderSyncAttempts",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderSyncAttempts_Orders_OrderId",
                table: "OrderSyncAttempts");

            migrationBuilder.DropIndex(
                name: "IX_OrderSyncAttempts_OrderId",
                table: "OrderSyncAttempts");
        }
    }
}
