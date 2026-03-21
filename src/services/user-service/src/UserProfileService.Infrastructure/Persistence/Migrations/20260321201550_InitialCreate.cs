using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserProfileService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Profile_FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Profile_LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Profile_Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Profile_DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Profile_PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Profile_Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Profile_City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Profile_Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Profile_ProfilePictureUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Profile_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    Timezone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "UTC"),
                    NotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EmailNotifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PushNotifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SmsNotifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReceivePromotions = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ReceiveNewsletter = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.UserProfileId);
                    table.ForeignKey(
                        name: "FK_UserPreferences_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Email",
                table: "UserProfiles",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_IsActive",
                table: "UserProfiles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
