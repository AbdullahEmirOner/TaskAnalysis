using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskAnalysis.DAL.Migrations
{
    public partial class taskai04 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepartmentAiAnalysisResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Directorate = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Task = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BestSolution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AutomationRate = table.Column<int>(type: "int", nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectIdeasJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponsiblePeopleJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentAiAnalysisResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonAiAnalysisResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SicilNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Task = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BestSolution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AutomationRate = table.Column<int>(type: "int", nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectIdeasJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponsiblePeopleJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonAiAnalysisResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAiAnalysisResults_Directorate_Department",
                table: "DepartmentAiAnalysisResults",
                columns: new[] { "Directorate", "Department" },
                unique: true,
                filter: "[Department] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonAiAnalysisResults_SicilNo",
                table: "PersonAiAnalysisResults",
                column: "SicilNo",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentAiAnalysisResults");

            migrationBuilder.DropTable(
                name: "PersonAiAnalysisResults");
        }
    }
}
