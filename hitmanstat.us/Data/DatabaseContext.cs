using Microsoft.EntityFrameworkCore;
using hitmanstat.us.Models;

namespace hitmanstat.us.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {}

        public DbSet<Event> Events { get; set; }

        public DbSet<UserReport> UserReports { get; set; }

        public DbSet<UserReportCounter> UserReportCounters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Event table
            // -----------
            modelBuilder.Entity<Event>().ToTable("Event");
            modelBuilder.Entity<Event>()
                .Property(e => e.Date)
                .HasDefaultValueSql("getdate()");

            // UserReport table
            // ----------------
            modelBuilder.Entity<UserReport>().ToTable("UserReport");
            modelBuilder.Entity<UserReport>()
                .Property(r => r.Date)
                .HasDefaultValueSql("getdate()");

            // UserReportCounter table
            // -----------------------
            modelBuilder.Entity<UserReportCounter>().ToTable("UserReportCounter");
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.H3steam)
                .HasDefaultValue(0);
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.H3epic)
                .HasDefaultValue(0);
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.H3xb)
                .HasDefaultValue(0);
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.H3ps)
                .HasDefaultValue(0);
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.H3sw)
                .HasDefaultValue(0);

            // Date definition
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.Date)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<UserReportCounter>()
                .HasIndex(c => c.Date)
                .IsUnique();
        }
    }
}
