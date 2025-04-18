Feature: Restful booker
    
Scenario Outline: Can create a booking using all encoding methods
    Given we create a booking using the encoding method <method>
    Then the booking from the result is identical to the one we created
    And we can retrieve the booking from the server
    
    Examples: 
    | method     |
    | json       |
    | xml        |
    | urlEncoded |

Scenario Outline: Can create a booking with different prices
    Given we create a booking with the price <price>
    Then the booking from the result is identical to the one we created
    And we can retrieve the booking from the server
    
    Examples: 
    | price      |
    | 0          |
    | 100        |
    | 100000     |
    | 2147483647 | // Largest integer for int32

Scenario Outline: Cannot create a booking with a negative price
    Given we create a booking with the price <price>
    Then the booking should not succeed
    
    Examples: 
    | price       |
    | -1          |
    | -50         |
    | -2147483648 | // Smallest integer for int32
    