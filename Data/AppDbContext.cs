using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleAPI.Models;

namespace VehicleAPI.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<VehicleBrand> VehicleBrands { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<VehicleYear> VehicleYears { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<PriceList> PriceLists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<User>().Property(u => u.UpdatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<VehicleBrand>().Property(vb => vb.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<VehicleBrand>().Property(vb => vb.UpdatedAt).HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<VehicleType>().Property(vt => vt.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<VehicleType>().Property(vt => vt.UpdatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<VehicleType>()
               .HasOne(vt => vt.Brand)
               .WithMany()
               .HasForeignKey(vt => vt.BrandId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VehicleModel>().Property(vm => vm.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<VehicleModel>().Property(vm => vm.UpdatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<VehicleModel>()
                .HasOne(vm => vm.Type)
                .WithMany()
                .HasForeignKey(vm => vm.TypeId);

            modelBuilder.Entity<VehicleYear>().Property(vy => vy.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<VehicleYear>().Property(vy => vy.UpdatedAt).HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<PriceList>().Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<PriceList>().Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<PriceList>()
                .HasOne(p => p.Year)
                .WithMany()
                .HasForeignKey(p => p.YearId);
            modelBuilder.Entity<PriceList>()
                .HasOne(p => p.Model)
                .WithMany()
                .HasForeignKey(p => p.ModelId);
        }
    }
}
