using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoAPI.Migrations
{
    /// <inheritdoc />
    public partial class amount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Transaction",
                newName: "Amount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Transaction",
                newName: "Price");
        }
    }
}
