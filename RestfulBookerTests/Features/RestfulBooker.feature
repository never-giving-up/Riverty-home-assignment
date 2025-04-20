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
    | price        |
    | 0            |
    | 10.5         |
    | 100.0001     |
    | 100          |
    | 100000       |
    | 2147483646.9 |
    | 2147483647   | // Largest integer for int32

Scenario Outline: Cannot create a booking with a negative price
    Given we create a booking with the price <price>
    Then the booking should not succeed
    
    Examples: 
    | price       |
    | -1          |
    | -3.75       |
    | -50.32334   |
    | -300        |
    | -2147483648 | // Smallest integer for int32
    
Scenario Outline: Can create a booking without the accept header
    Given we create a booking without adding the accept header and using the encoding method <method>
    Then the booking from the result is identical to the one we created
    
    Examples: 
    | method     |
    | json       |
    | xml        |
    | urlEncoded |

Scenario Outline: We can get bookings by name
    Given we create a booking using the first name <first> and the last name <last>
    Then we can retrieve the booking from the server using the name filtering
    
    Examples: 
        | first   | last   |
        | sally   | onally |
        | Jon     | Øberg  |
        |         |        |
        | 1234åøæ | !!\n   |
        
