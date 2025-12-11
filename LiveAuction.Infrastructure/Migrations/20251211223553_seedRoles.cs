using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LiveAuction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class seedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "19cd9391-a0d0-44ea-bf4f-502e26af9d86", "29cd9391-a0d0-44ea-bf4f-502e26af9d86", "Customer", "CUSTOMER" },
                    { "48ba6ac8-5656-4496-a2e9-bfb5401f22bd", "28ba6ac8-5656-4496-a2e9-bfb5401f22bd", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "19cd9391-a0d0-44ea-bf4f-502e26af9d86");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "48ba6ac8-5656-4496-a2e9-bfb5401f22bd");
        }
    }
}
