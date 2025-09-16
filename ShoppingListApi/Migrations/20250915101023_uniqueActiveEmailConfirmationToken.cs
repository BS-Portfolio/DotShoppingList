using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingListApi.Migrations
{
    /// <inheritdoc />
    public partial class uniqueActiveEmailConfirmationToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_EnumIndex",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_EmailConfirmationTokens_IsUsed",
                table: "EmailConfirmationTokens");

            migrationBuilder.DropIndex(
                name: "IX_EmailConfirmationTokens_Token",
                table: "EmailConfirmationTokens");

            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_Key",
                table: "ApiKeys");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_EnumIndex",
                table: "UserRoles",
                column: "EnumIndex",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserRoleTitle",
                table: "UserRoles",
                column: "UserRoleTitle",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfirmationTokens_IsUsed",
                table: "EmailConfirmationTokens",
                column: "IsUsed",
                unique: true,
                filter: "[IsUsed] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfirmationTokens_Token",
                table: "EmailConfirmationTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_Key",
                table: "ApiKeys",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_EnumIndex",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserRoleTitle",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_EmailConfirmationTokens_IsUsed",
                table: "EmailConfirmationTokens");

            migrationBuilder.DropIndex(
                name: "IX_EmailConfirmationTokens_Token",
                table: "EmailConfirmationTokens");

            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_Key",
                table: "ApiKeys");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_EnumIndex",
                table: "UserRoles",
                column: "EnumIndex");

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfirmationTokens_IsUsed",
                table: "EmailConfirmationTokens",
                column: "IsUsed");

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfirmationTokens_Token",
                table: "EmailConfirmationTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_Key",
                table: "ApiKeys",
                column: "Key");
        }
    }
}
