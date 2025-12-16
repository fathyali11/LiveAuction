using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveAuction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentBidderToAuction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentBidderId",
                table: "Auctions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_CurrentBidderId",
                table: "Auctions",
                column: "CurrentBidderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_AspNetUsers_CurrentBidderId",
                table: "Auctions",
                column: "CurrentBidderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_AspNetUsers_CurrentBidderId",
                table: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_CurrentBidderId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "CurrentBidderId",
                table: "Auctions");
        }
    }
}
