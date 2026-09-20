// Copyright © Erickson Lopez. MIT License.
using Microsoft.EntityFrameworkCore;

namespace EricksonLopez.Result.EntityFrameworkCore.Tests;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestItem> Items => Set<TestItem>();
}
