using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Persistence.Migrations
{
    public partial class FixVacationClientsNaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vacation_clients_client_ClientsId",
                table: "vacation_clients");

            migrationBuilder.DropForeignKey(
                name: "FK_vacation_clients_vacation_VacationsId",
                table: "vacation_clients");

            migrationBuilder.RenameColumn(
                name: "VacationsId",
                table: "vacation_clients",
                newName: "vacation_id");

            migrationBuilder.RenameColumn(
                name: "ClientsId",
                table: "vacation_clients",
                newName: "client_id");

            migrationBuilder.RenameIndex(
                name: "IX_vacation_clients_VacationsId",
                table: "vacation_clients",
                newName: "IX_vacation_clients_vacation_id");

            migrationBuilder.AddForeignKey(
                name: "fk_vacation_clients_client_id",
                table: "vacation_clients",
                column: "client_id",
                principalTable: "client",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vacation_clients_vacation_id",
                table: "vacation_clients",
                column: "vacation_id",
                principalTable: "vacation",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vacation_clients_client_id",
                table: "vacation_clients");

            migrationBuilder.DropForeignKey(
                name: "fk_vacation_clients_vacation_id",
                table: "vacation_clients");

            migrationBuilder.RenameColumn(
                name: "vacation_id",
                table: "vacation_clients",
                newName: "VacationsId");

            migrationBuilder.RenameColumn(
                name: "client_id",
                table: "vacation_clients",
                newName: "ClientsId");

            migrationBuilder.RenameIndex(
                name: "IX_vacation_clients_vacation_id",
                table: "vacation_clients",
                newName: "IX_vacation_clients_VacationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_vacation_clients_client_ClientsId",
                table: "vacation_clients",
                column: "ClientsId",
                principalTable: "client",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vacation_clients_vacation_VacationsId",
                table: "vacation_clients",
                column: "VacationsId",
                principalTable: "vacation",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
