using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DeeDeeAuction.Persistence;
using DeeDeeAuction.Services;
using DeeDeeAuction.Model;

var builder = WebApplication.CreateBuilder(args);

// 1) Register services (ทั้งหมดต้องมาก่อน Build)
var cs = builder.Configuration.GetConnectionString("Default")
         ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
         ?? "Host=localhost;Port=5433;Database=DeeDeeAuction;Username=admin;Password=P@ssw0rd";

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(cs));
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DeeDee Auction API", Version = "v1" });
});
builder.Services.AddCors(o => o.AddPolicy("_dev",
    p => p.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
          .AllowAnyHeader().AllowAnyMethod().AllowCredentials()
));

// 2) Build (ทำหลังลงทะเบียนครบแล้ว)
var app = builder.Build();

// 3) Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("_dev");

// 4) Migrate/Seed (หลัง Build เท่านั้น)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    // await Seed.RunAsync(db);
}

// 5) Endpoints
app.MapGet("/api/auctions", async (IAuctionService svc, int page = 1, int pageSize = 20, string? sort = null, CancellationToken ct = default)
    => Results.Ok(await svc.GetActiveAsync(page, pageSize, sort, ct)));

app.MapGet("/api/auctions/search", async (IAuctionService svc, string q, bool onlyActive = true, CancellationToken ct = default)
    => Results.Ok(await svc.SearchAsync(q, onlyActive, ct)));

app.MapGet("/api/auctions/{id:guid}", async (IAuctionService svc, Guid id, CancellationToken ct = default)
    => (await svc.GetByIdAsync(id, ct)) is { } a ? Results.Ok(a) : Results.NotFound());

app.MapPost("/api/bids", async (IBidService svc, PlaceBidBody body, CancellationToken ct = default) =>
{
    var r = await svc.PlaceAsync(body.AuctionId, body.BidderId, body.Amount, ct);
    return r.Success ? Results.Ok(new { bidId = r.BidId }) : Results.BadRequest(new { error = r.Error });
});

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
