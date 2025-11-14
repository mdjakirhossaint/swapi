using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoowGoodWeb.Migrations
{
    /// <inheritdoc />
    public partial class AgentMasterIdaddedinthefinancialSetUpService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AgentMasterId",
                table: "SgFinancialSetups",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SgFinancialSetups_AgentMasterId",
                table: "SgFinancialSetups",
                column: "AgentMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_SgFinancialSetups_SgAgentMasters_AgentMasterId",
                table: "SgFinancialSetups",
                column: "AgentMasterId",
                principalTable: "SgAgentMasters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SgFinancialSetups_SgAgentMasters_AgentMasterId",
                table: "SgFinancialSetups");

            migrationBuilder.DropIndex(
                name: "IX_SgFinancialSetups_AgentMasterId",
                table: "SgFinancialSetups");

            migrationBuilder.DropColumn(
                name: "AgentMasterId",
                table: "SgFinancialSetups");
        }
    }
}
