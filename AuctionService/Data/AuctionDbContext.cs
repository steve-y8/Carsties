using AuctionService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDbContext : DbContext
{
	public AuctionDbContext(DbContextOptions options) : base(options)
	{
	}

	public DbSet<Auction> Auctions { get; set; }

    /// <summary>
    /// Persist publishing messages to a message outbox
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <see cref="Program"/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Need to perform migration again after adding the following codes
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
