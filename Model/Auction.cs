namespace DeeDeeAuction.Model
{
    public enum AuctionType
    {
        English = 0,   // บิดเพิ่มขึ้นทีละ step
        Dutch = 1,     // ราคาลดลงตามเวลา (ถ้าจะใช้ทีหลัง)
        Sealed = 2     // ยื่นซอง (ไม่แสดงราคาคู่แข่ง)
    }

    public enum AuctionStatus
    {
        Draft = 0,
        Active = 1,
        Ended = 2,
        Cancelled = 3
    }

    public class Auction
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ItemId { get; set; }
        public Item Item { get; set; } = default!;

        public AuctionType Type { get; set; } = AuctionType.English;
        public AuctionStatus Status { get; set; } = AuctionStatus.Active;

        public decimal StartingPrice { get; set; }
        public decimal? BuyoutPrice { get; set; }
        public decimal BidIncrement { get; set; } = 50m;

        public string Currency { get; set; } = "THB";

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // นำไปคำนวณ current price: max(Bids.Amount) ?? StartingPrice
        public List<Bid> Bids { get; set; } = new();
    }
}