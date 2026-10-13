using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedDeletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "EndpointCheck");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EndpointCheck",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Endpoint",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Endpoint",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "EndpointCheck");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Endpoint");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Endpoint");

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "EndpointCheck",
                type: "integer",
                nullable: true);
        }
    }
}
