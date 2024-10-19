using System;

namespace pinjamdulu_backbone.Models
{
    public class Review
    {
        public Guid ReviewId { get; set; }
        public Guid BookingId { get; set; }
        public short Rating { get; set; }
        public string ReviewText { get; set; }
        public DateTime ReviewDate { get; set; }

        // Additional properties for UI
        public string ReviewerName { get; set; }
        public byte[] ReviewerProfilePicture { get; set; }
    }
}