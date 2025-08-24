﻿using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Dtos.JWTDTOs
{
    public class RefreshTokenRequestDTO
    {
        [Required(ErrorMessage = "Refresh Token is required.")]
        public string RefreshToken { get; set; } = null!;
        [Required(ErrorMessage = "Client Id is required.")]
        public string ClientId { get; set; } = null!;
    }
}
