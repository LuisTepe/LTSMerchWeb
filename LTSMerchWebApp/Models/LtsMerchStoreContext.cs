using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace LTSMerchWebApp.Models;

public partial class LtsMerchStoreContext : DbContext
{
    public LtsMerchStoreContext()
    {
    }

    public LtsMerchStoreContext(DbContextOptions<LtsMerchStoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Color> Colors { get; set; }

    public virtual DbSet<EmailConfirmation> EmailConfirmations { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderStatusType> OrderStatusTypes { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PaymentStatusType> PaymentStatusTypes { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductOption> ProductOptions { get; set; }

    public virtual DbSet<RoleType> RoleTypes { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=lts_merch_store;user=atepezano;password=atepezano1234", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.28-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PRIMARY");

            entity.ToTable("carts");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.CartId)
                .HasColumnType("int(11)")
                .HasColumnName("cart_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("carts_ibfk_1");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PRIMARY");

            entity.ToTable("cart_items");

            entity.HasIndex(e => e.CartId, "cart_id");

            entity.HasIndex(e => e.ProductOptionId, "product_option_id");

            entity.Property(e => e.CartItemId)
                .HasColumnType("int(11)")
                .HasColumnName("cart_item_id");
            entity.Property(e => e.CartId)
                .HasColumnType("int(11)")
                .HasColumnName("cart_id");
            entity.Property(e => e.ProductOptionId)
                .HasColumnType("int(11)")
                .HasColumnName("product_option_id");
            entity.Property(e => e.Quantity)
                .HasColumnType("int(11)")
                .HasColumnName("quantity");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("cart_items_ibfk_1");

            entity.HasOne(d => d.ProductOption).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductOptionId)
                .HasConstraintName("cart_items_ibfk_2");
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.ColorId).HasName("PRIMARY");

            entity.ToTable("colors");

            entity.Property(e => e.ColorId)
                .HasColumnType("int(11)")
                .HasColumnName("color_id");
            entity.Property(e => e.ColorName)
                .HasMaxLength(50)
                .HasColumnName("color_name");
        });

        modelBuilder.Entity<EmailConfirmation>(entity =>
        {
            entity.HasKey(e => e.ConfirmationId).HasName("PRIMARY");

            entity.ToTable("email_confirmations");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.ConfirmationId)
                .HasColumnType("int(11)")
                .HasColumnName("confirmation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IsConfirmed)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_confirmed");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .HasColumnName("token");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.EmailConfirmations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("email_confirmations_ibfk_1");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.FaqId).HasName("PRIMARY");

            entity.ToTable("faqs");

            entity.Property(e => e.FaqId)
                .HasColumnType("int(11)")
                .HasColumnName("faq_id");
            entity.Property(e => e.Answer)
                .HasColumnType("text")
                .HasColumnName("answer");
            entity.Property(e => e.Question)
                .HasColumnType("text")
                .HasColumnName("question");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PRIMARY");

            entity.ToTable("notifications");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.NotificationId)
                .HasColumnType("int(11)")
                .HasColumnName("notification_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_read");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("notifications_ibfk_1");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.ToTable("orders");

            entity.HasIndex(e => e.StatusTypeId, "status_type_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.OrderId)
                .HasColumnType("int(11)")
                .HasColumnName("order_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.ShippingAddress)
                .HasColumnType("text")
                .HasColumnName("shipping_address");
            entity.Property(e => e.StatusTypeId)
                .HasColumnType("int(11)")
                .HasColumnName("status_type_id");
            entity.Property(e => e.Total)
                .HasPrecision(10, 2)
                .HasColumnName("total");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("user_id");

            entity.HasOne(d => d.StatusType).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StatusTypeId)
                .HasConstraintName("orders_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("orders_ibfk_1");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PRIMARY");

            entity.ToTable("order_details");

            entity.HasIndex(e => e.OrderId, "order_id");

            entity.HasIndex(e => e.ProductOptionId, "product_option_id");

            entity.Property(e => e.OrderDetailId)
                .HasColumnType("int(11)")
                .HasColumnName("order_detail_id");
            entity.Property(e => e.OrderId)
                .HasColumnType("int(11)")
                .HasColumnName("order_id");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductOptionId)
                .HasColumnType("int(11)")
                .HasColumnName("product_option_id");
            entity.Property(e => e.Quantity)
                .HasColumnType("int(11)")
                .HasColumnName("quantity");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_details_ibfk_1");

            entity.HasOne(d => d.ProductOption).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductOptionId)
                .HasConstraintName("order_details_ibfk_2");
        });

        modelBuilder.Entity<OrderStatusType>(entity =>
        {
            entity.HasKey(e => e.StatusTypeId).HasName("PRIMARY");

            entity.ToTable("order_status_types");

            entity.Property(e => e.StatusTypeId)
                .HasColumnType("int(11)")
                .HasColumnName("status_type_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PRIMARY");

            entity.ToTable("payments");

            entity.HasIndex(e => e.OrderId, "order_id");

            entity.HasIndex(e => e.PaymentMethodId, "payment_method_id");

            entity.HasIndex(e => e.PaymentStatusTypeId, "payment_status_type_id");

            entity.Property(e => e.PaymentId)
                .HasColumnType("int(11)")
                .HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId)
                .HasColumnType("int(11)")
                .HasColumnName("order_id");
            entity.Property(e => e.PaymentMethodId)
                .HasColumnType("int(11)")
                .HasColumnName("payment_method_id");
            entity.Property(e => e.PaymentStatusTypeId)
                .HasColumnType("int(11)")
                .HasColumnName("payment_status_type_id");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("payments_ibfk_1");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentMethodId)
                .HasConstraintName("payments_ibfk_2");

            entity.HasOne(d => d.PaymentStatusType).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentStatusTypeId)
                .HasConstraintName("payments_ibfk_3");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PRIMARY");

            entity.ToTable("payment_methods");

            entity.Property(e => e.PaymentMethodId)
                .HasColumnType("int(11)")
                .HasColumnName("payment_method_id");
            entity.Property(e => e.MethodName)
                .HasMaxLength(50)
                .HasColumnName("method_name");
        });

        modelBuilder.Entity<PaymentStatusType>(entity =>
        {
            entity.HasKey(e => e.PaymentStatusTypeId).HasName("PRIMARY");

            entity.ToTable("payment_status_types");

            entity.Property(e => e.PaymentStatusTypeId)
                .HasColumnType("int(11)")
                .HasColumnName("payment_status_type_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("products");

            entity.Property(e => e.ProductId)
                .HasColumnType("int(11)")
                .HasColumnName("product_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Stock)
                .HasColumnType("int(11)")
                .HasColumnName("stock");
        });

        modelBuilder.Entity<ProductOption>(entity =>
        {
            entity.HasKey(e => e.ProductOptionId).HasName("PRIMARY");

            entity.ToTable("product_options");

            entity.HasIndex(e => e.ColorId, "color_id");

            entity.HasIndex(e => e.ProductId, "product_id");

            entity.HasIndex(e => e.SizeId, "size_id");

            entity.Property(e => e.ProductOptionId)
                .HasColumnType("int(11)")
                .HasColumnName("product_option_id");
            entity.Property(e => e.ColorId)
                .HasColumnType("int(11)")
                .HasColumnName("color_id");
            entity.Property(e => e.ProductId)
                .HasColumnType("int(11)")
                .HasColumnName("product_id");
            entity.Property(e => e.SizeId)
                .HasColumnType("int(11)")
                .HasColumnName("size_id");
            entity.Property(e => e.Stock)
                .HasColumnType("int(11)")
                .HasColumnName("stock");

            entity.HasOne(d => d.Color).WithMany(p => p.ProductOptions)
                .HasForeignKey(d => d.ColorId)
                .HasConstraintName("product_options_ibfk_3");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductOptions)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("product_options_ibfk_1");

            entity.HasOne(d => d.Size).WithMany(p => p.ProductOptions)
                .HasForeignKey(d => d.SizeId)
                .HasConstraintName("product_options_ibfk_2");
        });

        modelBuilder.Entity<RoleType>(entity =>
        {
            entity.HasKey(e => e.RoleTypeId).HasName("PRIMARY");

            entity.ToTable("role_types");

            entity.Property(e => e.RoleTypeId)
                .HasColumnType("int(11)")
                .HasColumnName("role_type_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("PRIMARY");

            entity.ToTable("sizes");

            entity.Property(e => e.SizeId)
                .HasColumnType("int(11)")
                .HasColumnName("size_id");
            entity.Property(e => e.SizeName)
                .HasMaxLength(10)
                .HasColumnName("size_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.RoleTypeId, "role_type_id");

            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phone_number");
            entity.Property(e => e.RoleTypeId)
                .HasColumnType("int(11)")
                .HasColumnName("role_type_id");

            entity.HasOne(d => d.RoleType).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleTypeId)
                .HasConstraintName("users_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
