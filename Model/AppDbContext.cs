using System.Reflection;
using Microsoft.EntityFrameworkCore;
using DeeDeeAuction.Model;
using Npgsql.EntityFrameworkCore.PostgreSQL; // สำหรับ UseXminAsConcurrencyToken (ถ้าเวอร์ชันรองรับ)


namespace DeeDeeAuction.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets หลัก
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Auction> Auctions => Set<Auction>();
    public DbSet<Bid> Bids => Set<Bid>();

    // ถ้ามีโมดูลอื่น ๆ (Kyc, Payment, Escrow, Shipment, Dispute, Review, Notification, Log ฯลฯ)
    // ให้ประกาศ DbSet เพิ่มตรงนี้ แล้วทำไฟล์ IEntityTypeConfiguration แยกตารางตามที่ออกแบบไว้

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // กำหนด xmin เป็น concurrency token ด้วย shadow property แบบชัดเจน
        modelBuilder.Entity<Auction>()
            .Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        modelBuilder.Entity<Bid>()
            .Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        modelBuilder.Entity<Item>()
            .HasOne(i => i.Category).WithMany()
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Item>()
            .HasOne(i => i.Seller).WithMany()
            .HasForeignKey(i => i.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Auction>()
            .HasOne(a => a.Item).WithMany()
            .HasForeignKey(a => a.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bid>()
            .HasOne(b => b.Auction).WithMany(a => a.Bids)
            .HasForeignKey(b => b.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bid>()
            .HasOne(b => b.Bidder).WithMany()
            .HasForeignKey(b => b.BidderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Item>().HasIndex(i => i.Title);
        modelBuilder.Entity<Auction>().HasIndex(a => a.EndTime);
        modelBuilder.Entity<Auction>().HasIndex(a => a.Status);
    }

}
