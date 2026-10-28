using LiveAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveAuction.Infrastructure.Presistence.EntitiesConfiguration;

internal class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.HasOne(a => a.User)
            .WithMany(x=>x.Transactions)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Supports net-hold calculations and wallet history for one auction.
        builder.HasIndex(a => new { a.UserId, a.AuctionId, a.TransactionType });

        builder.HasIndex(a => new { a.UserId, a.CreateAt });
    }
}
