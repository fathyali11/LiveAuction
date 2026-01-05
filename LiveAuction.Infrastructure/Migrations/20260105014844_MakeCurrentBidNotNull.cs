using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveAuction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeCurrentBidNotNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobId",
                table: "Auctions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentBid",
                table: "Auctions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Auctions_CurrentBid",
                table: "Auctions",
                sql: "[CurrentBid] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Auctions_StartingBid",
                table: "Auctions",
                sql: "[StartingBid] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Auctions_CurrentBid",
                table: "Auctions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Auctions_StartingBid",
                table: "Auctions");

            migrationBuilder.AlterColumn<string>(
                name: "JobId",
                table: "Auctions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentBid",
                table: "Auctions",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
