using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoMonitoring.DataProcessor.Migrations
{
    /// <inheritdoc />
    public partial class InitSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Snapshots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Sma7 = table.Column<decimal>(type: "numeric", nullable: true),
                    Sma21 = table.Column<decimal>(type: "numeric", nullable: true),
                    SupportLevel = table.Column<decimal>(type: "numeric", nullable: true),
                    ResistanceLevel = table.Column<decimal>(type: "numeric", nullable: true),
                    Volatility = table.Column<double>(type: "double precision", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Change24h = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snapshots", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Snapshots");
        }
    }
}
