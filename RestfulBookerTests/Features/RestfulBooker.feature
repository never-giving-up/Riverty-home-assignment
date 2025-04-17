Feature: Restful booker
    
Scenario Outline: Can create a booking using all encoding methods
    Given we create a booking using the encoding method <method>
    Then the booking from the result is identical to the one we created
    
    Examples: 
    | method     |
    | json       |
    | xml        |
    | urlEncoded |