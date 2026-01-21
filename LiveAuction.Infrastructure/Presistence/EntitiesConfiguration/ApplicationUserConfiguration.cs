using LiveAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveAuction.Infrastructure.Presistence.EntitiesConfiguration;

internal class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.TotalBalance)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.LockedBalance)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_Users_TotalBalance", "[TotalBalance] >= 0"));

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_Users_LockedBalance", "[LockedBalance] >= 0"));

    }
}
