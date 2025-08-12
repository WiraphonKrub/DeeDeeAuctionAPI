namespace DeeDeeAuction.Services;

public interface IBidService
{
    Task<PlaceBidResult> PlaceAsync(Guid auctionId, Guid bidderId, decimal amount, CancellationToken ct = default);
    Task<List<BidView>> GetByAuctionAsync(Guid auctionId, CancellationToken ct = default);
}

public record PlaceBidResult(bool Success, string? Error, Guid? BidId)
{
    public static PlaceBidResult Ok(Guid id) => new(true, null, id);
    public static PlaceBidResult Fail(string msg) => new(false, msg, null);
}

public record BidView(Guid Id, Guid AuctionId, Guid BidderId, decimal Amount, DateTime PlacedAt);
