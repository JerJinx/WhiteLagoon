using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Utility
{
    public static class Constants
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public static int VillaRoomsAvailableCount(
            int villaId,
            List<VillaNumber> villaNumberList,
            DateOnly checkInDate,
            int nights,
            List<Booking> bookings
        )
        {
            List<int> bookingInDate = new();

            int finalAvailableRoomForAllNights = int.MaxValue;

            var roomsInVilla = villaNumberList.Where(v => v.VillaId == villaId).Count();
            
            for (int i = 0; i < nights; i++)
            {
                var villasBooked = bookings.Where(b => 
                    b.CheckInDate <= checkInDate.AddDays(i) && 
                    b.CheckOutDate > checkInDate.AddDays(i) &&
                    b.VillaId == villaId);
                foreach (var booking in villasBooked)
                {
                    if (!bookingInDate.Contains(booking.Id))
                    {
                        bookingInDate.Add(booking.Id);
                    }
                }

                var totalAvailableRooms = roomsInVilla - bookingInDate.Count;

                if (totalAvailableRooms == 0)
                {
                    return 0; // No rooms available
                }
                else
                {
                    if (finalAvailableRoomForAllNights > totalAvailableRooms)
                    {
                        finalAvailableRoomForAllNights = totalAvailableRooms;
                    }
                }
            }

            return finalAvailableRoomForAllNights;
        }

    }
}
