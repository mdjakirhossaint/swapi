using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoowGoodWeb.Migrations
{
    /// <inheritdoc />
    public partial class Notification_Table_changed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Message",
                table: "SgNotification",
                newName: "MessageForReceiver");

            migrationBuilder.AddColumn<string>(
                name: "MessageForCreator",
                table: "SgNotification",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageForCreator",
                table: "SgNotification");

            migrationBuilder.RenameColumn(
                name: "MessageForReceiver",
                table: "SgNotification",
                newName: "Message");
        }
    }
}
