using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoowGoodWeb.Migrations
{
    /// <inheritdoc />
    public partial class platformPackageManagementTableadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SgPlatformPackageManagements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageRequestCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlatformPackageId = table.Column<long>(type: "bigint", nullable: true),
                    PatientProfileId = table.Column<long>(type: "bigint", nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PackageFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AppointmentStatus = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AppointmentPaymentStatus = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SgPlatformPackageManagements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SgPlatformPackageManagements_SgPatientProfiles_PatientProfileId",
                        column: x => x.PatientProfileId,
                        principalTable: "SgPatientProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SgPlatformPackageManagements_SgPlatformPackages_PlatformPackageId",
                        column: x => x.PlatformPackageId,
                        principalTable: "SgPlatformPackages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SgPlatformPackageManagements_PatientProfileId",
                table: "SgPlatformPackageManagements",
                column: "PatientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SgPlatformPackageManagements_PlatformPackageId",
                table: "SgPlatformPackageManagements",
                column: "PlatformPackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SgPlatformPackageManagements");
        }
    }
}
