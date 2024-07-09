using InfoTrackSearchModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace InfoTrackSearchData.Context;

public class InfoTrackDbContext : DbContext
{
    public InfoTrackDbContext(DbContextOptions<InfoTrackDbContext> options) : base(options) { }

    public DbSet<SearchResult> SearchResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var valueComparer = new ValueComparer<List<int>>(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        modelBuilder.Entity<SearchResult>()
            .Property(e => e.Positions)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList())
            .Metadata
            .SetValueComparer(valueComparer);
    }
}