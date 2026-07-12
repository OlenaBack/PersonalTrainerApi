using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Data;

namespace PersonalTrainer.Api.Tests.Unit.TestSupport;

public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
