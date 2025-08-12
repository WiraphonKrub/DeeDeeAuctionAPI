using DeeDeeAuction.Model;
using DeeDeeAuction.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeeDeeAuction.Services;

public class AuctionService : IAuctionService
{
    private readonly AppDbContext _db;

    public AuctionService(AppDbContext db) => _db = db;

    public async Task<List<AuctionListView>> GetActiveAsync(int page = 1, int pageSize = 20, string? sort = null, CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Auctions.AsNoTracking()
            .Include(a => a.Item)
            .Include(a => a.Bids)
            .Where(a => a.Status == AuctionStatus.Active && a.EndTime > DateTime.UtcNow);

        q = sort switch
        {
            "endTime.desc" => q.OrderByDescending(a => a.EndTime),
            _ => q.OrderBy(a => a.EndTime)
        };

        return await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AuctionListView(
                a.Id,
                a.Item.Title!,
                (a.Bids.Max(b => (decimal?)b.Amount) ?? a.StartingPrice),
                a.Currency,
                a.EndTime
            ))
            .ToListAsync(ct);
    }

    public async Task<List<AuctionListView>> SearchAsync(string q, bool onlyActive = true, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q)) return new();

        var terms = q.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var query = _db.Auctions.AsNoTracking()
            .Include(a => a.Item).ThenInclude(i => i.Category)
            .Include(a => a.Bids)
            .AsQueryable();

        if (onlyActive)
            query = query.Where(a => a.Status == AuctionStatus.Active && a.EndTime > DateTime.UtcNow);

        foreach (var t in terms)
        {
            var like = $"%{t}%";
            query = query.Where(a =>
                EF.Functions.ILike(a.Item.Title!, like) ||
                (a.Item.Description != null && EF.Functions.ILike(a.Item.Description, like)) ||
                EF.Functions.ILike(a.Item.Category.Name, like)
            );
        }

        return await query
            .OrderByDescending(a => a.Bids.Count)
            .Select(a => new AuctionListView(
                a.Id,
                a.Item.Title!,
                (a.Bids.Max(b => (decimal?)b.Amount) ?? a.StartingPrice),
                a.Currency,
                a.EndTime
            ))
            .ToListAsync(ct);
    }

    public async Task<AuctionDetailView?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Auctions.AsNoTracking()
            .Include(a => a.Item)
            .Include(a => a.Bids)
            .Where(a => a.Id == id)
            .Select(a => new AuctionDetailView(
                a.Id,
                a.Item.Title!,
                a.Item.Description,
                a.StartingPrice,
                a.BuyoutPrice,
                (a.Bids.Max(b => (decimal?)b.Amount) ?? a.StartingPrice),
                a.Currency,
                a.StartTime,
                a.EndTime,
                a.Status,
                a.Type
            ))
            .SingleOrDefaultAsync(ct);
    }
}
