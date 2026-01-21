using LiveAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveAuction.Infrastructure.Presistence.EntitiesConfiguration;

internal class WatchListItemConfiguration : IEntityTypeConfiguration<WatchListItem>
{
    public void Configure(EntityTypeBuilder<WatchListItem> builder)
    {
        builder.Property(a => a.CurrentBid)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_WatchListItems_WatchListId", "[WatchListId] > 0"));
    }
}