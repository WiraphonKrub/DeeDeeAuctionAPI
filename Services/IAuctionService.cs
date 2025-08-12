using DeeDeeAuction.Model;

namespace DeeDeeAuction.Services;

public interface IAuctionService
{
    /// <summary>รายการประมูลที่ยัง Active เรียงตามเวลาปิด (รองรับ paging/sort)</summary>
    Task<List<AuctionListView>> GetActiveAsync(int page = 1, int pageSize = 20, string? sort = null, CancellationToken ct = default);

    /// <summary>ค้นหาด้วยคำหลักจากชื่อ/คำอธิบาย/หมวดหมู่ (PostgreSQL ILIKE)</summary>
    Task<List<AuctionListView>> SearchAsync(string q, bool onlyActive = true, CancellationToken ct = default);

    /// <summary>รายละเอียดการประมูล</summary>
    Task<AuctionDetailView?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

/// <summary>DTO สำหรับหน้า List/Search</summary>
public record AuctionListView(
    Guid Id,
    string Title,
    decimal CurrentPrice,
    string Currency,
    DateTime EndTime
);

/// <summary>DTO สำหรับหน้า Detail</summary>
public record AuctionDetailView(
    Guid Id,
    string Title,
    string? Description,
    decimal StartingPrice,
    decimal? BuyoutPrice,
    decimal CurrentPrice,
    string Currency,
    DateTime StartTime,
    DateTime EndTime,
    AuctionStatus Status,
    AuctionType Type
);
