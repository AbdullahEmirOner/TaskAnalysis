using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskAnalysis.DAL.Migrations
{
    public partial class taskai07 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DirectorateTaskAnalysisResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Directorate = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResultJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectorateTaskAnalysisResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectorateTaskAnalysisResults_Directorate",
                table: "DirectorateTaskAnalysisResults",
                column: "Directorate",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DirectorateTaskAnalysisResults");
        }
    }
}
