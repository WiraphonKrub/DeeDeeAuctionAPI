using DeeDeeAuction.Model;
using DeeDeeAuction.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeeDeeAuction.Services;

public class BidService : IBidService
{
    private readonly AppDbContext _db;

    public BidService(AppDbContext db) => _db = db;

    public async Task<List<BidView>> GetByAuctionAsync(Guid auctionId, CancellationToken ct = default)
    {
        return await _db.Bids.AsNoTracking()
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.PlacedAt)
            .Select(b => new BidView(b.Id, b.AuctionId, b.BidderId, b.Amount, b.PlacedAt))
            .ToListAsync(ct);
    }

    // เดิม: คงสัญญาเดิมไว้
    public async Task<PlaceBidResult> PlaceAsync(Guid auctionId, Guid bidderId, decimal amount, CancellationToken ct = default)
    {
        var r = await PlaceWithPreviewAsync(auctionId, bidderId, amount, ct);
        return r.Success ? PlaceBidResult.Ok(r.BidId!.Value) : PlaceBidResult.Fail(r.Error!);
    }

    // ใหม่: วางบิด + คืนพรีวิวพร้อมข้อมูลรูปสินค้า
    public async Task<PlaceBidPreviewResult> PlaceWithPreviewAsync(Guid auctionId, Guid bidderId, decimal amount, CancellationToken ct = default)
    {
        var auction = await _db.Auctions
            .Include(a => a.Bids)
            .Include(a => a.Item)
            .SingleOrDefaultAsync(a => a.Id == auctionId, ct);

        if (auction is null)
            return FailPreview("Auction not found.");

        // เตรียมรูป (ถ้ามี) ให้ทุกกรณีตั้งแต่แรก
        var itemImageUrl = await ResolveItemImageUrlAsync(auction.ItemId, ct);

        if (auction.Status != AuctionStatus.Active || auction.EndTime <= DateTime.UtcNow)
            return FailPreview("Auction is not active or already ended.", auction, itemImageUrl);

        if (bidderId == auction.Item.SellerId)
            return FailPreview("Seller cannot bid on own auction.", auction, itemImageUrl);

        var current = auction.Bids.Count == 0 ? auction.StartingPrice : auction.Bids.Max(b => b.Amount);
        var minNext = current + auction.BidIncrement;
        if (amount < minNext)
            return FailPreview($"amount must be >= {minNext}", auction, itemImageUrl);

        var bid = new Bid
        {
            AuctionId = auction.Id,
            BidderId = bidderId,
            Amount = amount,
            PlacedAt = DateTime.UtcNow
        };

        _db.Bids.Add(bid);

        try
        {
            await _db.SaveChangesAsync(ct); // ถ้าเปิดใช้ xmin จะช่วยกันชน
        }
        catch (DbUpdateConcurrencyException)
        {
            return FailPreview("Concurrency conflict. Please retry.", auction, itemImageUrl);
        }

        // auto-end ถ้าถึง buyout
        if (auction.BuyoutPrice.HasValue && amount >= auction.BuyoutPrice.Value)
        {
            auction.Status = AuctionStatus.Ended;
            await _db.SaveChangesAsync(ct);
        }

        return new PlaceBidPreviewResult(
            Success: true,
            Error: null,
            BidId: bid.Id,
            AuctionId: auction.Id,
            Amount: bid.Amount,
            PlacedAt: bid.PlacedAt,
            ItemId: auction.ItemId,
            ItemTitle: auction.Item.Title,
            ItemImageUrl: itemImageUrl
        );
    }

    private PlaceBidPreviewResult FailPreview(string msg, Auction? auction = null, string? itemImageUrl = null) =>
        new(
            Success: false,
            Error: msg,
            BidId: null,
            AuctionId: auction?.Id,
            Amount: null,
            PlacedAt: null,
            ItemId: auction?.ItemId,
            ItemTitle: auction?.Item.Title,
            ItemImageUrl: itemImageUrl
        );

    // คืน URL รูปสินค้า (อ่านจาก Item.ImageUrl)
    private async Task<string?> ResolveItemImageUrlAsync(Guid itemId, CancellationToken ct)
    {
        // ถ้าเก็บรูปใน Item.ImageUrl
        var url = await _db.Items.AsNoTracking()
            .Where(i => i.Id == itemId)
            .Select(i => i.ImageUrl)   // ต้องมี property นี้ใน Model/Item.cs
            .FirstOrDefaultAsync(ct);

        return url;

        // ----- ถ้าเก็บรูปใน ItemImages ให้ใช้โค้ดนี้แทน -----
        // return await _db.ItemImages.AsNoTracking()
        //     .Where(img => img.ItemId == itemId)
        //     .OrderByDescending(img => img.IsPrimary)
        //     .ThenBy(img => img.SortOrder)
        //     .Select(img => img.Url)
        //     .FirstOrDefaultAsync(ct);
    }
}
