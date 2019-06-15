using Microsoft.EntityFrameworkCore;
using hitmanstat.us.Models;

namespace hitmanstat.us.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>().ToTable("Event");
            modelBuilder.Entity<Event>()
                .Property(e => e.Date)
                .HasDefaultValueSql("getdate()");
        }
    }
}
