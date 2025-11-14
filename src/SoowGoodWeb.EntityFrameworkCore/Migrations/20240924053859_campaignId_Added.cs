using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoowGoodWeb.Migrations
{
    /// <inheritdoc />
    public partial class campaignId_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CampaignId",
                table: "SgFinancialSetups",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SgFinancialSetups_CampaignId",
                table: "SgFinancialSetups",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_SgFinancialSetups_SgCampaigns_CampaignId",
                table: "SgFinancialSetups",
                column: "CampaignId",
                principalTable: "SgCampaigns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SgFinancialSetups_SgCampaigns_CampaignId",
                table: "SgFinancialSetups");

            migrationBuilder.DropIndex(
                name: "IX_SgFinancialSetups_CampaignId",
                table: "SgFinancialSetups");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "SgFinancialSetups");
        }
    }
}
