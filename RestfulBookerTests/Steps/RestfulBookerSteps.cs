using System.Globalization;
using System.Threading.Tasks;
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
    public async Task GivenWeCreateABookingUsingTheEncodingMethod(string encodingMethod)
    {
        var requestBodyType = RequestBodyTypeHelper.FromString(encodingMethod);
        var bookingToCreate = CreateDefaultBooking();
        var result = await _restfulBookerClient!.CreateBooking(bookingToCreate, requestBodyType);
        StoreBookingContext(bookingToCreate, result!);
    }

    private void StoreBookingContext(Booking bookingToCreate, BookingResponseDTO result)
    {
        _scenarioContext[ScenarioContextKeys.BookingToCreate] = bookingToCreate;
        _scenarioContext[ScenarioContextKeys.BookingResponse] = result.booking;
        _scenarioContext[ScenarioContextKeys.CreatedBookingId] = result.bookingid;
    }

    [Given("we create a booking with the price (.*)")]
    public async Task GivenWeCreateABookingWithThePrice(double price)
    {
        var bookingToCreate = CreateDefaultBooking();
        bookingToCreate.totalprice = price;
        var result = await _restfulBookerClient!.CreateBooking(bookingToCreate);
        
        StoreBookingContext(bookingToCreate, result!);
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
            booking2.Should().BeEquivalentTo(booking1, options => options
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
        
        bookingFromOriginalCreateRequest.Should().NotBeNull();
        var bookingFromServer = await _restfulBookerClient!.GetBooking(createdBookingId);

        AreBookingsIdentical(originalBooking, bookingFromServer).Should().BeTrue();
        AreBookingsIdentical(bookingFromOriginalCreateRequest, bookingFromServer).Should().BeTrue();
    }

    [Given("we create a booking without adding the accept header and using the encoding method (.*)")]
    public async Task GivenWeCreateABookingWithoutAddingTheAcceptHeaderAndUsingTheEncodingMethod(string encodingMethod)
    {
        var requestBodyType = RequestBodyTypeHelper.FromString(encodingMethod);
        var bookingToCreate = CreateDefaultBooking();
       
        var bookingResponse = await _restfulBookerClient!.CreateBookingWithoutAcceptHeader(bookingToCreate, requestBodyType);
        bookingResponse.Should().NotBeNull();
        StoreBookingContext(bookingToCreate, bookingResponse);

    }

    [Given("we create a booking using the first name (.*) and the last name (.*) and encoding method (.*)")]
    public async Task GivenWeCreateABookingUsingTheFirstNameAndTheLastNameUsing(string first, string last, string encodingMethod)
    {
        var requestBodyType = RequestBodyTypeHelper.FromString(encodingMethod);
        var bookingToCreate = CreateDefaultBooking();
        bookingToCreate.firstname = first;
        bookingToCreate.lastname = last;
        
        var result = await _restfulBookerClient!.CreateBooking(bookingToCreate, requestBodyType);
        StoreBookingContext(bookingToCreate, result!);
    }

    [Then("we can retrieve the booking from the server using the name filtering")]
    public async Task ThenWeCanRetrieveTheBookingFromTheServerUsingTheNameFiltering()
    {
        var originalBooking = _scenarioContext[ScenarioContextKeys.BookingToCreate] as Booking;
        var createdBookingId = (int) _scenarioContext[ScenarioContextKeys.CreatedBookingId];
        
        originalBooking.Should().NotBeNull();
        var originalBookingFirstName = originalBooking.firstname;
        var originalBookingLastName = originalBooking.lastname;

        var bookingIdsFromServer = await _restfulBookerClient!.GetBookingIdsByName(originalBookingFirstName,
            originalBookingLastName);
        
        bookingIdsFromServer.Any(booking => booking.bookingid == createdBookingId).Should().BeTrue();
    }

    [Given("we create a booking using the checkin date (.*) and the checkout date (.*)")]
    public async Task GivenWeCreateABookingUsingTheCheckinDateAndTheCheckoutDate(string checkinDate, string checkoutDate)
    {
        CheckDateIsInProperFormat(checkinDate).Should().BeTrue();
        CheckDateIsInProperFormat(checkoutDate).Should().BeTrue();
        
        var bookingToCreate = CreateDefaultBooking();
        bookingToCreate.bookingdates.checkin = checkinDate;
        bookingToCreate.bookingdates.checkout = checkoutDate;
        
        var result = await _restfulBookerClient!.CreateBooking(bookingToCreate);
        StoreBookingContext(bookingToCreate, result!);
    }

    private static bool CheckDateIsInProperFormat(string dateString)
    {
        const string correctFormat = "yyyy-MM-dd";
        var isFormatCorrect = 
                DateTime.TryParseExact(
                    dateString,
                    correctFormat,
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.None,
                    out var _);
        return isFormatCorrect;
    }

    [Then("we can retrieve the booking from the server using the date filtering")]
    public async Task ThenWeCanRetrieveTheBookingFromTheServerUsingTheDateFiltering()
    {
        var originalBooking = _scenarioContext[ScenarioContextKeys.BookingToCreate] as Booking;
        var createdBookingId = (int) _scenarioContext[ScenarioContextKeys.CreatedBookingId];
        
        originalBooking.Should().NotBeNull();
        // var originalBookingCheckin = originalBooking.bookingdates.checkin;
        var originalBookingCheckin = "1999-01-01";
        
        var originalBookingCheckout = originalBooking.bookingdates.checkout;

        var bookingIdsFromServer = await _restfulBookerClient!.GetBookingIdsByBookingDates(originalBookingCheckin,
            originalBookingCheckout);
        
        bookingIdsFromServer.Any(booking => booking.bookingid == createdBookingId).Should().BeTrue();
    }

    [Then("we can retrieve the booking from the server using the checkin date filtering")]
    public async Task ThenWeCanRetrieveTheBookingFromTheServerUsingTheCheckinDateFiltering()
    {
        var originalBooking = _scenarioContext[ScenarioContextKeys.BookingToCreate] as Booking;
        var createdBookingId = (int) _scenarioContext[ScenarioContextKeys.CreatedBookingId];
        
        originalBooking.Should().NotBeNull();
        var originalBookingCheckin = originalBooking.bookingdates.checkin;

        var bookingIdsFromServer = await _restfulBookerClient!.GetBookingIdsByBookingDates(originalBookingCheckin,
            null);
        
        bookingIdsFromServer.Any(booking => booking.bookingid == createdBookingId).Should().BeTrue();
        
    }
    
    [Then("we can retrieve the booking from the server using the checkout date filtering")]
    public async Task ThenWeCanRetrieveTheBookingFromTheServerUsingTheCheckoutDateFiltering()
    {
        var originalBooking = _scenarioContext[ScenarioContextKeys.BookingToCreate] as Booking;
        var createdBookingId = (int) _scenarioContext[ScenarioContextKeys.CreatedBookingId];
        
        originalBooking.Should().NotBeNull();
        var originalBookingCheckout = originalBooking.bookingdates.checkout;

        var bookingIdsFromServer = await _restfulBookerClient!.GetBookingIdsByBookingDates(null,
            originalBookingCheckout);
        
        bookingIdsFromServer.Any(booking => booking.bookingid == createdBookingId).Should().BeTrue();
        
    }

    [When("we update the first name to (.*) using the encoding method (.*)")]
    public async Task WhenWeUpdateTheFirstNameTo(string newFirstName, string encodingMethod)
    {
        var requestBodyType = RequestBodyTypeHelper.FromString(encodingMethod);
        var newBooking = _scenarioContext[ScenarioContextKeys.BookingToCreate] as Booking;
        var originalBookingId = (int) _scenarioContext[ScenarioContextKeys.CreatedBookingId];
        newBooking.Should().NotBeNull();
        
        newBooking.firstname = newFirstName;
        var updateBookingResponse = await _restfulBookerClient!.UpdateBooking(newBooking, originalBookingId, requestBodyType);
        
        StoreBookingContext(newBooking, new BookingResponseDTO {booking = updateBookingResponse, bookingid = originalBookingId});
    }
}

public static class ScenarioContextKeys
{
    public const string BookingToCreate = "BookingToCreate";
    public const string BookingResponse = "BookingResponse";
    public const string CreatedBookingId = "CreatedBookingId";
}