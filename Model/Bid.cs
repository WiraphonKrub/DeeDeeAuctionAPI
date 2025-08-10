namespace DeeDeeAuction.Model
{
    public class Bid
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AuctionId { get; set; }
        public Auction Auction { get; set; } = default!;

        public Guid BidderId { get; set; }
        public User Bidder { get; set; } = default!;

        public decimal Amount { get; set; }
        public DateTime PlacedAt { get; set; } = DateTime.UtcNow;

        // หมายเหตุ: Concurrency/locking ใช้ xmin ใน DbContext (ไม่ต้องใส่ RowVersion ที่นี่)
    }
}