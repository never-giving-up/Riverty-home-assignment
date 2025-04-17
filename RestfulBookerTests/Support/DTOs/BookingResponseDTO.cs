using System.Text.Json.Serialization;

namespace RestfulBookerTests.Support.DTOs;

public class BookingResponseDTO
{
    public int bookingid { get; set; }
    public Booking booking { get; set; }
}