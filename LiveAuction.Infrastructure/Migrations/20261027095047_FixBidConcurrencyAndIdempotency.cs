using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveAuction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBidConcurrencyAndIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Bids_AuctionId",
                table: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Bids_BidderId",
                table: "Bids");

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Bids",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExtensionCount",
                table: "Auctions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Auctions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_AuctionId_TransactionType",
                table: "Transactions",
                columns: new[] { "UserId", "AuctionId", "TransactionType" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_CreateAt",
                table: "Transactions",
                columns: new[] { "UserId", "CreateAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Bids_AuctionId_BidTime",
                table: "Bids",
                columns: new[] { "AuctionId", "BidTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Bids_BidderId_AuctionId_IdempotencyKey",
                table: "Bids",
                columns: new[] { "BidderId", "AuctionId", "IdempotencyKey" },
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_Status_StartTime_EndTime",
                table: "Auctions",
                columns: new[] { "Status", "StartTime", "EndTime" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_LockedBalanceLessThanOrEqualToTotalBalance",
                table: "AspNetUsers",
                sql: "[LockedBalance] <= [TotalBalance]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId_AuctionId_TransactionType",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId_CreateAt",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Bids_AuctionId_BidTime",
                table: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Bids_BidderId_AuctionId_IdempotencyKey",
                table: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_Status_StartTime_EndTime",
                table: "Auctions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_LockedBalanceLessThanOrEqualToTotalBalance",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "ExtensionCount",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_AuctionId",
                table: "Bids",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_BidderId",
                table: "Bids",
                column: "BidderId");
        }
    }
}
