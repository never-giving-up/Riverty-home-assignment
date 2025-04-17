using System.Diagnostics;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RestfulBookerTests.Support.DTOs;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RestfulBookerTests.Support;

public class RequestBodySerializer
{
    
    private const string TemplateBookingAsEncodedUrl = "firstname={0}&lastname={1}&totalprice={2}&depositpaid={3}&bookingdates%5Bcheckin%5D={4}&bookingdates%5Bcheckout%5D={5}";
    
    public string Serialize(object body, RequestBodyType type)
    {
        switch (type)
        {
            case RequestBodyType.Json:
                return JsonSerializer.Serialize(body);
            case RequestBodyType.Xml:
                return SerializeIntoXml(body);
            case RequestBodyType.UrlEncodedForm:
                return SerializeToUrlEncodedForm(body);
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private static string SerializeToUrlEncodedForm(object body)
    {
        if (body is not Booking booking)
            throw new ArgumentException("Body must be of type Booking");
        
        return string.Format(TemplateBookingAsEncodedUrl, booking.firstname,
            booking.lastname, booking.totalprice,
            booking.depositpaid, booking.bookingdates.checkin,
            booking.bookingdates.checkout);
    }

    private static string SerializeIntoXml(object body)
    {
        var xmlWithEmptyNamespaceAndDeclaration = SerializeToXmlWithEmptyNamespace(body);
        return RemoveXmlDeclaration(xmlWithEmptyNamespaceAndDeclaration);
    }

    private static string SerializeToXmlWithEmptyNamespace(object body)
    {
        StringWriter? stringWriter = null;
        try
        {
            var emptyNamespace = new XmlSerializerNamespaces(); // Prevents the root element from being prefixed with the namespace
            emptyNamespace.Add("", "");
        
            stringWriter = new StringWriter();
            var serializer = new XmlSerializer(body.GetType());
            serializer.Serialize(stringWriter, body, emptyNamespace);
            
            return stringWriter.ToString();
        }
        catch
        {
            stringWriter?.Dispose();
            throw;
        }
    }

    private static string RemoveXmlDeclaration(string xml)
    {
        return xml.Remove(0, xml.IndexOf('\n') + 1);
    }
}