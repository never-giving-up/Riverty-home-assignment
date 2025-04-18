using Flurl.Http;
using Flurl;
using RestfulBookerTests.Support.DTOs;

namespace RestfulBookerTests.Support;

public class RestfulBookerClient
{
    private readonly string _baseUrl = "https://restful-booker.herokuapp.com";
    private readonly string _authPath = "/auth";
    private readonly string _bookingsPath = "booking";
    
    private string _token;
    public RestfulBookerClient()
    {
        // Since the constructor cannot use async methods, we use this blocking call
        var result = Authenticate().GetAwaiter().GetResult(); 
        _token = result.Token;
    }

    private string GetAuthUrl()
    {
        return _baseUrl + _authPath;
    }
    
    private async Task<AuthTokenDTO> Authenticate()
    {
        return await GetAuthUrl()
            .WithHeader("Content-Type", "application/json")
            .PostJsonAsync(body: new
            {
                username = "admin",
                password = "password123"
            })
            .ReceiveJson<AuthTokenDTO>();
    }

    public async Task<BookingResponseDTO> CreateBooking(Booking booking, RequestBodyType bodyType = RequestBodyType.Json)
    {
        try
        {
            var request = _baseUrl
                .AppendPathSegment(_bookingsPath)
                .WithHeader("Accept", "application/json");
            request = AddContentTypeHeader(request, bodyType);
            var requestBody = new RequestBodySerializer().Serialize(booking, bodyType);
            return await request.PostStringAsync(requestBody).ReceiveJson<BookingResponseDTO>();
        }
        catch (FlurlHttpException ex)
        {
            var err = await ex.GetResponseStringAsync();
            Console.Write($"Error returned from {ex.Call.Request.Url}: {err}");
        }

        return null;
    }
    
    private IFlurlRequest AddContentTypeHeader(IFlurlRequest request, RequestBodyType bodyType)
    {
        switch (bodyType)
        {
            case RequestBodyType.Json:
                return request.WithHeader("Content-Type", "application/json");
            case RequestBodyType.Xml:
                return request.WithHeader("Content-Type", "text/xml");
            case RequestBodyType.UrlEncodedForm:
                return request.WithHeader("Content-Type", "application/x-www-form-urlencoded");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public async Task<Booking?> GetBooking(int id)
    {
        try
        {
            return await _baseUrl
                .AppendPathSegment(_bookingsPath)
                .AppendPathSegment(id.ToString())
                .WithHeader("Accept", "application/json")
                .GetJsonAsync<Booking>();
        } catch (FlurlHttpException ex)
        {
            var err = await ex.GetResponseStringAsync();
            Console.Write($"Error returned from {ex.Call.Request.Url}: {err}");
            throw;
        }
    }
}