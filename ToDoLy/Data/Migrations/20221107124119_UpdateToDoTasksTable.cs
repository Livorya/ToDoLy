using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoLy.Data.Migrations
{
    public partial class UpdateToDoTasksTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "ToDoTasks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ToDoTasks");
        }
    }
}
