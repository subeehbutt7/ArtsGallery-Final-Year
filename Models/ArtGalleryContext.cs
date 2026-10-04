using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ArtGalleryFinal.Models;

public partial class ArtGalleryContext : DbContext
{
    public ArtGalleryContext()
    {
    }

    public ArtGalleryContext(DbContextOptions<ArtGalleryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<ArtCategory> ArtCategories { get; set; }

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<Artwork> Artworks { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Shipment> Shipments { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    public virtual DbSet<Complaint> Complaints { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=MUHAMMADNAJAM\\SQLEXPRESS;Database=ArtGallery;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.ToTable("Admin");
        });

        modelBuilder.Entity<ArtCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId);

            entity.ToTable("ArtCategory");
        });

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.ToTable("Artist");

            entity.HasOne(d => d.Admin).WithMany(p => p.Artists)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK_Artist_Admin");
        });

        modelBuilder.Entity<Artwork>(entity =>
        {
            entity.HasKey(e => e.ArtId);

            entity.ToTable("Artwork");

            entity.HasOne(d => d.Artist).WithMany(p => p.Artworks)
                .HasForeignKey(d => d.ArtistId)
                .HasConstraintName("FK_Artwork_Artist");

            entity.HasOne(d => d.Category).WithMany(p => p.Artworks)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Artwork_ArtCategory");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customer");

            entity.HasOne(d => d.Admin).WithMany(p => p.Customers)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK_Customer_Admin");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Order");

            entity.HasOne(d => d.Admin).WithMany(p => p.Orders)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK_Order_Admin");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Order_Customer");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.ToTable("OrderDetail");

            entity.HasOne(d => d.Art).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ArtId)
                .HasConstraintName("FK_OrderDetail_Artwork");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderDetail_Order");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payment");

            entity.HasOne(d => d.Admin).WithMany(p => p.Payments)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK_Payment_Admin");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Payment_Order");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.ToTable("Rating");

            entity.HasOne(d => d.Art).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.ArtId)
                .HasConstraintName("FK_Rating_Artwork");

            entity.HasOne(d => d.Customer).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Rating_Customer");
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.ToTable("Shipment");

            entity.HasOne(d => d.Admin).WithMany(p => p.Shipments)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK_Shipment_Admin");

            entity.HasOne(d => d.Order).WithMany(p => p.Shipments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Shipment_Order");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId);
            entity.ToTable("PaymentTransactions");

            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");

            entity.HasOne(d => d.Order).WithMany()
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_PaymentTransaction_Order");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
