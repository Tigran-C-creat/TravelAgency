using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Persistence.Migrations
{
    public partial class AddClientToVacationRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "message",
                table: "vacation_request",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "client_id",
                table: "vacation_request",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_vacation_request_client_id",
                table: "vacation_request",
                column: "client_id");

            migrationBuilder.AddForeignKey(
                name: "FK_vacation_request_client_client_id",
                table: "vacation_request",
                column: "client_id",
                principalTable: "client",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vacation_request_client_client_id",
                table: "vacation_request");

            migrationBuilder.DropIndex(
                name: "IX_vacation_request_client_id",
                table: "vacation_request");

            migrationBuilder.DropColumn(
                name: "client_id",
                table: "vacation_request");

            migrationBuilder.AlterColumn<string>(
                name: "message",
                table: "vacation_request",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
