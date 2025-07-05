using Opti.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace Opti.Data
{
    public class ApplicationDbContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<ProductionOrder> ProductionOrders { get; set; }
        public DbSet<MachineLog> MachineLogs { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<Payment> Payments { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring the primary keys using HasKey
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<Product>()
                .HasKey(p => p.ProductId);

            modelBuilder.Entity<Machine>()
                .HasKey(m => m.MachineId);

            modelBuilder.Entity<ProductionOrder>()
                .HasKey(po => po.OrderId);

            modelBuilder.Entity<MachineLog>()
                .HasKey(ml => ml.LogId);

            modelBuilder.Entity<CustomerOrder>()
                .HasKey(co => co.OrderId);

            modelBuilder.Entity<Payment>()
                .HasKey(p => p.PaymentId);

            // Configuring Foreign Key relationships
            modelBuilder.Entity<ProductionOrder>()
                .HasOne(po => po.Product)
                .WithMany()
                .HasForeignKey(po => po.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Define delete behavior

            modelBuilder.Entity<ProductionOrder>()
                .HasOne(po => po.User)  // Worker processing the production order
                .WithMany()
                .HasForeignKey(po => po.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MachineLog>()
                .HasOne(ml => ml.Machine)
                .WithMany()
                .HasForeignKey(ml => ml.MachineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MachineLog>()
                .HasOne(ml => ml.User) // Worker logging the machine action
                .WithMany()
                .HasForeignKey(ml => ml.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerOrder>()
                .HasOne(co => co.Product)
                .WithMany()
                .HasForeignKey(co => co.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerOrder>()
                .HasOne(co => co.User) // Customer placing the order
                .WithMany()
                .HasForeignKey(co => co.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.ProductionOrder)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User) // Customer making the payment
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuring the Payment Table to store transaction-related info directly
            modelBuilder.Entity<Payment>()
                .Property(p => p.TransactionExternalId)
                .IsRequired(); // External transaction ID is required (e.g., PayMongo ID)

            modelBuilder.Entity<Payment>()
                .Property(p => p.Gateway)
                .IsRequired(); // Payment gateway is required (e.g., PayMongo, PayPal)

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .IsRequired(); // Status of the payment (e.g., Completed, Pending)

        }
    }
}