using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketBookingApp.Migrations
{
    /// <inheritdoc />
    public partial class mig6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingSeats_Seats_SeatId",
                table: "BookingSeats");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingSeats_Seats_SeatId",
                table: "BookingSeats",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingSeats_Seats_SeatId",
                table: "BookingSeats");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingSeats_Seats_SeatId",
                table: "BookingSeats",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
