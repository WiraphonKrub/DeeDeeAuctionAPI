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

    public async Task<PlaceBidResult> PlaceAsync(Guid auctionId, Guid bidderId, decimal amount, CancellationToken ct = default)
    {
        var auction = await _db.Auctions
            .Include(a => a.Bids)
            .Include(a => a.Item)
            .SingleOrDefaultAsync(a => a.Id == auctionId, ct);

        if (auction is null)
            return PlaceBidResult.Fail("Auction not found.");

        if (auction.Status != AuctionStatus.Active || auction.EndTime <= DateTime.UtcNow)
            return PlaceBidResult.Fail("Auction is not active or already ended.");

        if (bidderId == auction.Item.SellerId)
            return PlaceBidResult.Fail("Seller cannot bid on own auction.");

        var current = auction.Bids.Count == 0 ? auction.StartingPrice : auction.Bids.Max(b => b.Amount);
        var minNext = current + auction.BidIncrement;

        if (amount < minNext)
            return PlaceBidResult.Fail($"amount must be >= {minNext}");

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
            await _db.SaveChangesAsync(ct); // optimistic concurrency ด้วย xmin (ตั้งใน AppDbContext)
        }
        catch (DbUpdateConcurrencyException)
        {
            return PlaceBidResult.Fail("Concurrency conflict. Please retry.");
        }

        // auto-end เมื่อถึง buyout
        if (auction.BuyoutPrice.HasValue && amount >= auction.BuyoutPrice.Value)
        {
            auction.Status = AuctionStatus.Ended;
            await _db.SaveChangesAsync(ct);
        }

        return PlaceBidResult.Ok(bid.Id);
    }
}
