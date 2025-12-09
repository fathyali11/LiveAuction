using Microsoft.EntityFrameworkCore;

namespace LiveAuction.Infrastructure.Presistence;
internal class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
