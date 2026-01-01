using LiveAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveAuction.Infrastructure.Presistence.EntitiesConfiguration;

internal class WatchListConfiguration : IEntityTypeConfiguration<WatchList>
{
    public void Configure(EntityTypeBuilder<WatchList> builder)
    {
        builder.HasIndex(wl => new {wl.Id,wl.UserId}).IsUnique();

        builder.HasMany(wl => wl.Items)
            .WithOne(wli => wli.WatchList)
            .HasForeignKey(wli => wli.WatchListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wl => wl.User)
            .WithOne(u => u.WatchList)
            .HasForeignKey<WatchList>(wl => wl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
