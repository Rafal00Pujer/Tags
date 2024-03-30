using Microsoft.EntityFrameworkCore;
using Tags.Models;

namespace Tags.Context;

public class TagsContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<TagModel> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
