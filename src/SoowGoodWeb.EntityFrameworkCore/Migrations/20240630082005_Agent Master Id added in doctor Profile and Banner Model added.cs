using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoowGoodWeb.Migrations
{
    /// <inheritdoc />
    public partial class AgentMasterIdaddedindoctorProfileandBannerModeladded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AgentMasterId",
                table: "SgDoctorProfiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SgBanners",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    createFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SgBanners", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SgDoctorProfiles_AgentMasterId",
                table: "SgDoctorProfiles",
                column: "AgentMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_SgDoctorProfiles_SgAgentMasters_AgentMasterId",
                table: "SgDoctorProfiles",
                column: "AgentMasterId",
                principalTable: "SgAgentMasters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SgDoctorProfiles_SgAgentMasters_AgentMasterId",
                table: "SgDoctorProfiles");

            migrationBuilder.DropTable(
                name: "SgBanners");

            migrationBuilder.DropIndex(
                name: "IX_SgDoctorProfiles_AgentMasterId",
                table: "SgDoctorProfiles");

            migrationBuilder.DropColumn(
                name: "AgentMasterId",
                table: "SgDoctorProfiles");
        }
    }
}
