using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveAuction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobIdToAuction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobId",
                table: "Auctions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobId",
                table: "Auctions");
        }
    }
}
