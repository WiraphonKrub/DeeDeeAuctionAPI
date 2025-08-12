namespace DeeDeeAuction.Model
{
    public record PlaceBidBody(Guid AuctionId, Guid BidderId, decimal Amount);
}
