namespace DeeDeeAuction.Model
{
    public class Item
    {
        public Guid Id { get; set; }

        public Guid SellerId { get; set; }
        public User Seller { get; set; } = default!;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        // ✅ เพิ่มฟิลด์เก็บ URL รูปหลัก
        public string? ImageUrl { get; set; }

        public ICollection<Auction> Auctions { get; set; } = new List<Auction>();
    }
}
