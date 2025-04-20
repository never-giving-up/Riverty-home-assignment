using System;
using System.Threading.Tasks;
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

    public async Task<BookingResponseDTO?> CreateBooking(Booking booking,
        RequestBodyType requestBodyType = RequestBodyType.Json)
    {
        return await CreateBooking(booking, requestBodyType, includeAcceptHeader: true);
    }

    public async Task<BookingResponseDTO?> CreateBookingWithoutAcceptHeader(Booking booking,
        RequestBodyType requestBodyType = RequestBodyType.Json)
    {
        return await CreateBooking(booking, requestBodyType, includeAcceptHeader: false);
    }

    private async Task<BookingResponseDTO?> CreateBooking(Booking booking,
        RequestBodyType bodyType, bool includeAcceptHeader = true)
    {
        try
        {
            var requestUrl = _baseUrl
                .AppendPathSegment(_bookingsPath);
            var request = AddContentTypeHeader(requestUrl, bodyType);
            if (includeAcceptHeader)
                request = request.WithHeader("Accept", "application/json");
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

    private IFlurlRequest AddContentTypeHeader(Url? url, RequestBodyType bodyType)
    {
        return bodyType switch
        {
            RequestBodyType.Json => url.WithHeader("Content-Type", "application/json"),
            RequestBodyType.Xml => url.WithHeader("Content-Type", "text/xml"),
            RequestBodyType.UrlEncodedForm => url.WithHeader("Content-Type", "application/x-www-form-urlencoded"),
            _ => throw new ArgumentOutOfRangeException()
        };
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

    public async Task<List<BookingId>> GetBookingIdsByName(string firstName, string lastName)
    {
        return await _baseUrl
            .AppendPathSegment(_bookingsPath)
            .AppendQueryParam("firstname", firstName)
            .AppendQueryParam("lastname", lastName)
            .WithHeader("Accept", "application/json")
            .GetJsonAsync<List<BookingId>>();
    }

    public async Task<List<BookingId>> GetBookingIdsByBookingDates(string? checkin, string? checkout)
    {
        var url = _baseUrl.AppendPathSegment(_bookingsPath);
        if (checkin != null)
            url.AppendQueryParam("checkin", checkin);
        if (checkout != null)
            url.AppendQueryParam("checkout", checkout);
        
        return await url
            .WithHeader("Accept", "application/json")
            .GetJsonAsync<List<BookingId>>();
    }

    public async Task<Booking> UpdateBooking(Booking newBooking, int originalBookingId, RequestBodyType requestBodyType = RequestBodyType.Json)
    {
       var url = _baseUrl.AppendPathSegment(_bookingsPath).AppendPathSegment(originalBookingId.ToString()); 
       var request = AddContentTypeHeader(url, requestBodyType);
       request = AddAuthToken(request);
       request = request.WithHeader("Accept", "application/json");
       
       var requestBody = new RequestBodySerializer().Serialize(newBooking, requestBodyType);
       return await request.PutStringAsync(requestBody).ReceiveJson<Booking>();
    }

    private IFlurlRequest AddAuthToken(IFlurlRequest request)
    {
        return request.WithHeader("Cookie", $"token={_token}");
    }
}