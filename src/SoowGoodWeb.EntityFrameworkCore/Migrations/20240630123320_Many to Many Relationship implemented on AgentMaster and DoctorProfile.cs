using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoowGoodWeb.Migrations
{
    /// <inheritdoc />
    public partial class ManytoManyRelationshipimplementedonAgentMasterandDoctorProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SgDoctorProfiles_SgAgentMasters_AgentMasterId",
                table: "SgDoctorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_SgDoctorProfiles_AgentMasterId",
                table: "SgDoctorProfiles");

            migrationBuilder.DropColumn(
                name: "AgentMasterId",
                table: "SgDoctorProfiles");

            migrationBuilder.CreateTable(
                name: "SgMasterDoctors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorProfileId = table.Column<long>(type: "bigint", nullable: true),
                    AgentMasterId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_SgMasterDoctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SgMasterDoctors_SgAgentMasters_AgentMasterId",
                        column: x => x.AgentMasterId,
                        principalTable: "SgAgentMasters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SgMasterDoctors_SgDoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "SgDoctorProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SgMasterDoctors_AgentMasterId",
                table: "SgMasterDoctors",
                column: "AgentMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_SgMasterDoctors_DoctorProfileId",
                table: "SgMasterDoctors",
                column: "DoctorProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SgMasterDoctors");

            migrationBuilder.AddColumn<long>(
                name: "AgentMasterId",
                table: "SgDoctorProfiles",
                type: "bigint",
                nullable: true);

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
    }
}
