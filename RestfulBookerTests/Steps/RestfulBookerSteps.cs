using FluentAssertions;
using Reqnroll;
using RestfulBookerTests.Support;
using RestfulBookerTests.Support.DTOs;

namespace RestfulBookerTests.Steps;

[Binding]
public class RestfulBookerSteps(ScenarioContext scenarioContext)
{
    private RestfulBookerClient? _restfulBookerClient;
    private readonly ScenarioContext _scenarioContext = scenarioContext;

    [BeforeScenario]
    public void BeforeScenario()
    {
        _restfulBookerClient = new RestfulBookerClient();
    }
    
    [Given("we create a booking using the encoding method (.*)")]
    public async Task GivenWeCreateABooking(string encodingMethod)
    {
        var requestBodyType = RequestBodyTypeHelper.FromString(encodingMethod);
        var bookingToCreate = CreateDefaultBooking();
        var result = await _restfulBookerClient!.CreateBooking(bookingToCreate, requestBodyType);
        StoreBookingContext(bookingToCreate, result);
    }

    private void StoreBookingContext(Booking bookingToCreate, BookingResponseDTO result)
    {
        _scenarioContext[ScenarioContextKeys.BookingToCreate] = bookingToCreate;
        _scenarioContext[ScenarioContextKeys.BookingResponse] = result.booking;
        _scenarioContext[ScenarioContextKeys.CreatedBookingId] = result.bookingid;
    }

    [Given("we create a booking with the price (.*)")]
    public async Task GivenWeCreateABookingWithThePrice(int price)
    {
        var bookingToCreate = CreateDefaultBooking();
        // Another possible edge case is using decimals
        // I'm not implementing this here for time considerations
        bookingToCreate.totalprice = price;
        var result = await _restfulBookerClient!.CreateBooking(bookingToCreate);
        
        StoreBookingContext(bookingToCreate, result);
    }
    
    [Then("the booking from the result is identical to the one we created")]
    public void ThenTheBookingFromTheResultIsIdenticalToTheOneWeCreated()
    {
        var bookingToCreate = _scenarioContext[ScenarioContextKeys.BookingToCreate] as Booking;
        var bookingResponse = _scenarioContext[ScenarioContextKeys.BookingResponse] as Booking;
        
        bookingToCreate.Should().NotBeNull();
        bookingResponse.Should().NotBeNull();
        AreBookingsIdentical(bookingToCreate, bookingResponse)
            .Should().BeTrue();
    }

    private static bool AreBookingsIdentical(Booking? booking1, Booking? booking2)
    {
        if (booking1 == null || booking2 == null)
            return false;

        try
        {
            booking1.Should().BeEquivalentTo(booking2, options => options
                .IncludingAllRuntimeProperties());
            return true;
        }
        catch (FluentAssertions.Execution.AssertionFailedException)
        {
            return false;
        }
    }

    private static Booking CreateDefaultBooking()
    {
        var booking = new Booking()
        {
            firstname = "Jim",
            lastname = "Brown",
            depositpaid = true,
            totalprice = 111,
            bookingdates = new BookingDates()
            {
                checkin = "2018-01-01",
                checkout = "2019-01-01"
            },
        };
        return booking;
    }

    [Then("the booking should not succeed")]
    public void ThenTheBookingShouldNotSucceed()
    {
        var bookingToCreate = _scenarioContext[ScenarioContextKeys.BookingToCreate] as Booking;
        var bookingResponse = _scenarioContext[ScenarioContextKeys.BookingResponse] as Booking;
        
        if(bookingToCreate != null && bookingResponse != null)
            AreBookingsIdentical(bookingToCreate, bookingResponse)
                .Should().BeFalse();
    }

    [Then("we can retrieve the booking from the server")]
    public async Task ThenWeCanRetrieveTheBookingFromTheServer()
    {
        var originalBooking = _scenarioContext[ScenarioContextKeys.BookingToCreate] as Booking;
        var bookingFromOriginalCreateRequest = _scenarioContext[ScenarioContextKeys.BookingResponse] as Booking;
        var createdBookingId = (int)_scenarioContext[ScenarioContextKeys.CreatedBookingId]!; // Since int is a primitive type, we need to cast it directly, instead of using the as operator
        
        // createdBookingId.Should().NotBeNull();
        bookingFromOriginalCreateRequest.Should().NotBeNull();
        var bookingFromServer = await _restfulBookerClient!.GetBooking(createdBookingId);

        AreBookingsIdentical(originalBooking, bookingFromServer).Should().BeTrue();
        AreBookingsIdentical(bookingFromOriginalCreateRequest, bookingFromServer).Should().BeTrue();
    }
}

public static class ScenarioContextKeys
{
    public const string BookingToCreate = "BookingToCreate";
    public const string BookingResponse = "BookingResponse";
    public const string CreatedBookingId = "CreatedBookingId";
}