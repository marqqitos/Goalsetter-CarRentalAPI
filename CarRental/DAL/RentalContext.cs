using CarRental.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRental.DAL
{
    public class RentalContext : DbContext
    {
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }
        public virtual DbSet<Rental> Rentals { get; set; }

        public RentalContext() { }

        public RentalContext(DbContextOptions<RentalContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Vehicle)
                .WithMany(v => v.Rentals)
                .HasForeignKey(r => r.VehicleID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Client)
                .WithMany(c => c.Rentals)
                .HasForeignKey(r => r.ClientID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
