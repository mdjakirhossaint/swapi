using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoowGoodWeb.Migrations
{
    /// <inheritdoc />
    public partial class addPlatformPackageFacilityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SgPlatformPackageFacilities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlateformPackageId = table.Column<long>(type: "bigint", nullable: true),
                    PlatformPackageId = table.Column<long>(type: "bigint", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_SgPlatformPackageFacilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SgPlatformPackageFacilities_SgPlatformPackages_PlatformPackageId",
                        column: x => x.PlatformPackageId,
                        principalTable: "SgPlatformPackages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SgPlatformPackageFacilities_PlatformPackageId",
                table: "SgPlatformPackageFacilities",
                column: "PlatformPackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SgPlatformPackageFacilities");
        }
    }
}
