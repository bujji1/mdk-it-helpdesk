using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MdkHelpdesk.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Seed-data only: moves sample addresses to the reserved example.com domain, adds a user with
    /// no tickets and an unassigned ticket. No schema changes.
    /// </summary>
    public partial class NeutralSeedDataWithUnassignedExamples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Email",
                value: "alice.johnson@example.com");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Email",
                value: "bob.martinez@example.com");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Email",
                value: "priya.patel@example.com");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name" },
                values: new object[] { 4, "daniel.kim@example.com", "Daniel Kim" });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Grant read/send-as permissions for legal-intake@example.com.");

            // Written by hand instead of InsertData: a database that is already in use may have
            // given Id 4 to a ticket created through the app. In that case the sample ticket is
            // added with the next free Id rather than failing the migration.
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM [Tickets] WHERE [Id] = 4)
                BEGIN
                    SET IDENTITY_INSERT [Tickets] ON;
                    INSERT INTO [Tickets] ([Id], [AssignedUserId], [CreatedAt], [Description], [Status], [Title], [UpdatedAt])
                    VALUES (4, NULL, '2026-11-02T09:30:00.0000000', N'New hire starts Monday; needs a laptop, an email account and VPN access set up.', N'Open', N'New starter needs laptop and accounts', '2026-11-02T09:30:00.0000000');
                    SET IDENTITY_INSERT [Tickets] OFF;
                END
                ELSE
                BEGIN
                    INSERT INTO [Tickets] ([AssignedUserId], [CreatedAt], [Description], [Status], [Title], [UpdatedAt])
                    VALUES (NULL, '2026-11-02T09:30:00.0000000', N'New hire starts Monday; needs a laptop, an email account and VPN access set up.', N'Open', N'New starter needs laptop and accounts', '2026-11-02T09:30:00.0000000');
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM [Tickets] WHERE [AssignedUserId] IS NULL AND [Title] = N'New starter needs laptop and accounts' AND [CreatedAt] = '2026-11-02T09:30:00.0000000';");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Grant read/send-as permissions for legal-intake@mdklegal.com.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Email",
                value: "alice.johnson@mdklegal.com");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Email",
                value: "bob.martinez@mdklegal.com");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Email",
                value: "priya.patel@mdklegal.com");
        }
    }
}
