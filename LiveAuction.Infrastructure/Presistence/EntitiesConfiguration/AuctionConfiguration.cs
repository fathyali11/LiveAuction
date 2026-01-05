using LiveAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveAuction.Infrastructure.Presistence.EntitiesConfiguration;

internal class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.JobId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        

        builder.Property(a => a.StartTime)
            .IsRequired();

        builder.Property(a => a.EndTime)
            .IsRequired();

        builder.Property(a => a.StartingBid)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.CurrentBid)
            .IsRequired()
            .HasColumnType("decimal(18,2)");


        builder.Property(a => a.CreatedById)
            .IsRequired();

        builder.HasMany(a => a.Bids)
            .WithOne(b => b.Auction)
            .HasForeignKey(b => b.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_Auctions_CurrentBid", "[CurrentBid] > 0"));

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_Auctions_StartingBid", "[StartingBid] > 0"));
    }
}