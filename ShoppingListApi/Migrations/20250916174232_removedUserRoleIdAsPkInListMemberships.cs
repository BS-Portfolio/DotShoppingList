using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingListApi.Migrations
{
    /// <inheritdoc />
    public partial class removedUserRoleIdAsPkInListMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ListMemberships",
                table: "ListMemberships");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ListMemberships",
                table: "ListMemberships",
                columns: new[] { "ShoppingListId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ListMemberships",
                table: "ListMemberships");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ListMemberships",
                table: "ListMemberships",
                columns: new[] { "ShoppingListId", "UserId", "UserRoleId" });
        }
    }
}
