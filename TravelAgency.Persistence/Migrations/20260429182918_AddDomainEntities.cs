using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Persistence.Migrations
{
    public partial class AddDomainEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    inn = table.Column<string>(type: "text", nullable: false),
                    legal_address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vacation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacation", x => x.id);
                    table.ForeignKey(
                        name: "FK_vacation_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vacation_clients",
                columns: table => new
                {
                    ClientsId = table.Column<Guid>(type: "uuid", nullable: false),
                    VacationsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacation_clients", x => new { x.ClientsId, x.VacationsId });
                    table.ForeignKey(
                        name: "FK_vacation_clients_client_ClientsId",
                        column: x => x.ClientsId,
                        principalTable: "client",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vacation_clients_vacation_VacationsId",
                        column: x => x.VacationsId,
                        principalTable: "vacation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vacation_request",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vacation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacation_request", x => x.id);
                    table.ForeignKey(
                        name: "FK_vacation_request_vacation_vacation_id",
                        column: x => x.vacation_id,
                        principalTable: "vacation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_client_company_id",
                table: "client",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_vacation_employee_id",
                table: "vacation",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_vacation_clients_VacationsId",
                table: "vacation_clients",
                column: "VacationsId");

            migrationBuilder.CreateIndex(
                name: "IX_vacation_request_vacation_id",
                table: "vacation_request",
                column: "vacation_id");

            migrationBuilder.AddForeignKey(
                name: "FK_client_company_company_id",
                table: "client",
                column: "company_id",
                principalTable: "company",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_client_company_company_id",
                table: "client");

            migrationBuilder.DropTable(
                name: "company");

            migrationBuilder.DropTable(
                name: "vacation_clients");

            migrationBuilder.DropTable(
                name: "vacation_request");

            migrationBuilder.DropTable(
                name: "vacation");

            migrationBuilder.DropIndex(
                name: "IX_client_company_id",
                table: "client");
        }
    }
}
