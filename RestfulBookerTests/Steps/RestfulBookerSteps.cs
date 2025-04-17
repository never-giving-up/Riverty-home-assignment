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
        _scenarioContext["BookingToCreate"] = bookingToCreate;
        _scenarioContext["BookingResponse"] = result.booking;
    }
    
    [Then("the booking from the result is identical to the one we created")]
    public void ThenTheBookingFromTheResultIsIdenticalToTheOneWeCreated()
    {
        var bookingToCreate = _scenarioContext["BookingToCreate"] as Booking;
        var bookingResponse = _scenarioContext["BookingResponse"] as Booking;
        
        bookingToCreate.Should().NotBeNull();
        bookingResponse.Should().NotBeNull();
        AssertBookingsAreIdentical(bookingToCreate, bookingResponse);
    }

    private static void AssertBookingsAreIdentical(Booking booking1, Booking booking2)
    {
        booking1.Should().NotBeNull();
        booking2.Should().NotBeNull();

        booking1.Should().BeEquivalentTo(booking2, options => options
            .IncludingAllRuntimeProperties());
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

}