using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveAuction.Infrastructure.Presistence.EntitiesConfiguration;

internal class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        var roles = new List<IdentityRole>
        {
            new() {
                Id = "48ba6ac8-5656-4496-a2e9-bfb5401f22bd",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "28ba6ac8-5656-4496-a2e9-bfb5401f22bd"
            },
            new() {
                Id = "19cd9391-a0d0-44ea-bf4f-502e26af9d86",
                Name = "Customer",
                NormalizedName = "CUSTOMER",
                ConcurrencyStamp = "29cd9391-a0d0-44ea-bf4f-502e26af9d86"
            }
        };
        builder.HasData(roles);
    }
}