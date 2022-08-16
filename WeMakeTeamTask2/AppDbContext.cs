using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace WeMakeTeamTask2
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        { }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        public DbSet<Entity> Entities { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "Test");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>().Property(e => e.OperationDate).
                HasConversion(
                v => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(v, DateTimeKind.Local)),
                v => DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(v, TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id)), DateTimeKind.Local));
        }
    }
}
