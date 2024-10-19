using System;

namespace pinjamdulu_backbone.Models
{
    public class Booking
    {
        public Guid BookingId { get; set; }
        public Guid GadgetId { get; set; }
        public Guid BorrowerId { get; set; }
        public Guid LenderId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime RentalStartDate { get; set; }
        public DateTime RentalEndDate { get; set; }
    }
}