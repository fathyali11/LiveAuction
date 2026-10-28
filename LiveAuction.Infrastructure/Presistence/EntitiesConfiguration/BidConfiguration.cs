using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveAuction.Infrastructure.Presistence.EntitiesConfiguration;

internal class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
        builder.Property(b => b.BidTime)
            .IsRequired();
        builder.Property(b => b.IdempotencyKey)
            .HasMaxLength(BidRules.IdempotencyKeyMaxLength);

        builder.HasOne(b => b.Auction)
            .WithMany(a => a.Bids)
            .HasForeignKey(b => b.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Bidder)
            .WithMany(b=>b.Bids)
            .HasForeignKey(b => b.BidderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Scope idempotency to the bidder and auction. A global unique key would let one client
        // preempt another client's key.
        builder.HasIndex(b => new { b.BidderId, b.AuctionId, b.IdempotencyKey })
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL");

        builder.HasIndex(b => new { b.AuctionId, b.BidTime });

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_Bids_AuctionId", "[AuctionId] > 0"));
    }
}
