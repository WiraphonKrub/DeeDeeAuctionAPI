namespace DeeDeeAuction.Model
{
    public enum ItemCondition
    {
        Unknown = 0,
        New = 1,
        LikeNew = 2,
        Used = 3,
        ForParts = 4
    }

    public class Item
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public ItemCondition Condition { get; set; } = ItemCondition.Unknown;

        // ความสัมพันธ์
        public Guid SellerId { get; set; }
        public User Seller { get; set; } = default!;

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = default!;
    }
}