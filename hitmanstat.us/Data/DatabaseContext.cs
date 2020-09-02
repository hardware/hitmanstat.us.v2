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
            modelBuilder.Entity<Event>().ToTable("Event");
            modelBuilder.Entity<Event>()
                .Property(e => e.Date)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<UserReport>().ToTable("UserReport");
            modelBuilder.Entity<UserReport>()
                .Property(r => r.Date)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<UserReportCounter>().ToTable("UserReportCounter");
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.H1pc)
                .HasDefaultValue(0);
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.H1xb)
                .HasDefaultValue(0);
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.H1ps)
                .HasDefaultValue(0);
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.H2pc)
                .HasDefaultValue(0);
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.H2xb)
                .HasDefaultValue(0);
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.H2ps)
                .HasDefaultValue(0);
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.H2st)
                .HasDefaultValue(0);
            modelBuilder.Entity<UserReportCounter>()
                .Property(c => c.Date)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<UserReportCounter>()
                .HasIndex(c => c.Date)
                .IsUnique();
        }
    }
}
