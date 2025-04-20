using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace RestfulBookerTests.Support.DTOs;

public class BookingDates
{
    [XmlElement("checkin")]
    public string checkin { get; set; }
    [XmlElement("checkout")]
    public string checkout { get; set; }
}

[XmlRoot("booking")]
public class Booking
{
    [XmlElement("firstname")]
    public string firstname { get; set; }
    [XmlElement("lastname")]
    public string lastname { get; set; }
    [XmlElement("totalprice")]
    public double totalprice { get; set; }
    [XmlElement("depositpaid")]
    public bool depositpaid { get; set; }
    [XmlElement("bookingdates")]
    public BookingDates bookingdates { get; set; }
    [XmlElement("additionalneeds")]
    public string additionalneeds { get; set; }
}