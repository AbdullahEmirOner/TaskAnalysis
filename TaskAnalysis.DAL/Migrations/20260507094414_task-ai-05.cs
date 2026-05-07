using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskAnalysis.DAL.Migrations
{
    public partial class taskai05 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutomationRate",
                table: "PersonAiAnalysisResults");

            migrationBuilder.DropColumn(
                name: "BestSolution",
                table: "PersonAiAnalysisResults");

            migrationBuilder.DropColumn(
                name: "ProjectIdeasJson",
                table: "PersonAiAnalysisResults");

            migrationBuilder.DropColumn(
                name: "Recommendation",
                table: "PersonAiAnalysisResults");

            migrationBuilder.DropColumn(
                name: "ResponsiblePeopleJson",
                table: "PersonAiAnalysisResults");

            migrationBuilder.RenameColumn(
                name: "Task",
                table: "PersonAiAnalysisResults",
                newName: "ResultJson");

            migrationBuilder.AddColumn<int>(
                name: "ChunkCount",
                table: "DepartmentAiAnalysisResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecordCount",
                table: "DepartmentAiAnalysisResults",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChunkCount",
                table: "DepartmentAiAnalysisResults");

            migrationBuilder.DropColumn(
                name: "RecordCount",
                table: "DepartmentAiAnalysisResults");

            migrationBuilder.RenameColumn(
                name: "ResultJson",
                table: "PersonAiAnalysisResults",
                newName: "Task");

            migrationBuilder.AddColumn<int>(
                name: "AutomationRate",
                table: "PersonAiAnalysisResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BestSolution",
                table: "PersonAiAnalysisResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectIdeasJson",
                table: "PersonAiAnalysisResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Recommendation",
                table: "PersonAiAnalysisResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePeopleJson",
                table: "PersonAiAnalysisResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
