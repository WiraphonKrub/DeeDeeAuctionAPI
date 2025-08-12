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

}
