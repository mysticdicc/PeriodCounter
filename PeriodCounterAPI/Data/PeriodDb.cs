using Microsoft.EntityFrameworkCore;
using PeriodLib;

namespace PeriodCounterAPI.Data
{
    public class PeriodDb : DbContext
    {
        public PeriodDb() { }
        public PeriodDb(DbContextOptions<PeriodDb> options) : base(options) { }
        public virtual DbSet<PeriodStartTime> StartTimes { get; set; }
        public virtual DbSet<DeviceRegistration> DeviceRegistrations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PeriodStartTime>(entity =>
            {
                entity.ToTable("perioddates");
            });

            modelBuilder.Entity<DeviceRegistration>(entity =>
            {
                entity.ToTable("deviceregistration");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
