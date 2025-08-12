namespace DeeDeeAuction.Model
{
    // Model/Bid.cs
    public class Bid
    {
        public Guid Id { get; set; }
        public Guid AuctionId { get; set; }
        public Auction Auction { get; set; } = default!;
        public Guid BidderId { get; set; }
        public User Bidder { get; set; } = default!;
        public decimal Amount { get; set; }

        // เพิ่ม
        public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    }

}