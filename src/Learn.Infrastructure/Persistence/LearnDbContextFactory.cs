using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Learn.Infrastructure.Persistence;

public class LearnDbContextFactory : IDesignTimeDbContextFactory<LearnDbContext>
{
    public LearnDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<LearnDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=LearnPlatform;Trusted_Connection=True");

        return new LearnDbContext(optionsBuilder.Options);
    }
}
