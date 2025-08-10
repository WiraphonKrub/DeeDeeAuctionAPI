namespace DeeDeeAuction.Model
{


    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Email { get; set; } = default!;
        public string? DisplayName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // ตัวอย่างฟิลด์เพิ่ม (ภายหลังอาจย้ายไปตาราง KYC/Identity)
        // public string? Phone { get; set; }
    }
}