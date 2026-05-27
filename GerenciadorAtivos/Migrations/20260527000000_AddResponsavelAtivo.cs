using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorAtivos.Migrations
{
    [Migration("20260527000000_AddResponsavelAtivo")]
    public partial class AddResponsavelAtivo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResponsavelId",
                table: "Ativos",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ativos_ResponsavelId",
                table: "Ativos",
                column: "ResponsavelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ativos_AspNetUsers_ResponsavelId",
                table: "Ativos",
                column: "ResponsavelId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ativos_AspNetUsers_ResponsavelId",
                table: "Ativos");

            migrationBuilder.DropIndex(
                name: "IX_Ativos_ResponsavelId",
                table: "Ativos");

            migrationBuilder.DropColumn(
                name: "ResponsavelId",
                table: "Ativos");
        }
    }
}
