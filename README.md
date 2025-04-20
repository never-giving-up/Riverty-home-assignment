# Riverty-home-assignment
This repo is for the home assignment from Riverty

This repo uses the Fluent Assertions package. According to its license, it's free for non-commercial use and since this is a home task, I don't mind using it.

# Assumptions I am making
Since I don't have a team to ask about expected behavior in edge cases, the following are assumptions I am making:
1. I am going to assume that we are using whole numbers for the total price of the booking.

# Bugs I found
1. You can create a booking with a negative total price
2. The server returns a 418 (I'm a teapot) response when trying to create bookings without adding the Accept header to the request