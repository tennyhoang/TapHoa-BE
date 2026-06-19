using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;

namespace TapHoa.Persistence.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Hub> Hubs => Set<Hub>();
    public DbSet<UserHub> UserHubs => Set<UserHub>();
    public DbSet<HubInventory> HubInventories => Set<HubInventory>();
    public DbSet<OrderClaim> OrderClaims => Set<OrderClaim>();
    public DbSet<OrderDamagedReport> OrderDamagedReports => Set<OrderDamagedReport>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<WalletTopupRequest> WalletTopupRequests => Set<WalletTopupRequest>();
    public DbSet<WithdrawRequest> WithdrawRequests => Set<WithdrawRequest>();
    public DbSet<FlashSaleSession> FlashSaleSessions => Set<FlashSaleSession>();
    public DbSet<FlashSaleItem> FlashSaleItems => Set<FlashSaleItem>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<PushToken> PushTokens => Set<PushToken>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<LoyaltyAccount> LoyaltyAccounts => Set<LoyaltyAccount>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.FullName).HasMaxLength(200);
            e.Property(u => u.Email).HasMaxLength(256);

            e.HasOne<Hub>()
             .WithMany()
             .HasForeignKey(u => u.AgentHubId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);

            e.HasOne(u => u.AssignedWarehouse)
             .WithMany(w => w.AssignedDrivers)
             .HasForeignKey(u => u.WarehouseId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);

            e.HasOne(u => u.ManagedWarehouse)
             .WithMany()
             .HasForeignKey(u => u.ManagedWarehouseId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            e.Property(p => p.DiscountPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.WalletAmountUsed).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            e.Property(o => o.Status).HasConversion<string>();
            e.Property(o => o.CancelReason).HasMaxLength(500);
            e.Property(o => o.DeliveryPhotoUrl).HasMaxLength(2048);
            e.Property(o => o.FailedDeliveryAttempts).HasDefaultValue(0);

            e.HasOne(o => o.AssignedDriver)
             .WithMany()
             .HasForeignKey(o => o.AssignedDriverId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        modelBuilder.Entity<InventoryTransaction>(e =>
        {
            e.Property(t => t.Type).HasConversion<string>();
            e.Property(t => t.Reason).HasMaxLength(500);
            e.HasOne(t => t.Product)
             .WithMany()
             .HasForeignKey(t => t.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(e =>
            e.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)"));

        modelBuilder.Entity<Category>(e =>
            e.HasOne(c => c.Parent)
             .WithMany(c => c.Children)
             .HasForeignKey(c => c.ParentId)
             .OnDelete(DeleteBehavior.Restrict));

        modelBuilder.Entity<Review>(e =>
        {
            e.HasIndex(r => new { r.UserId, r.ProductId }).IsUnique();
            e.Property(r => r.IsApproved).HasDefaultValue(true);
            e.Property(r => r.Sentiment).HasMaxLength(20).HasDefaultValue("Neutral");
        });

        modelBuilder.Entity<Hub>(e =>
        {
            e.Property(h => h.Name).HasMaxLength(200).IsRequired();
            e.Property(h => h.Address).HasMaxLength(500).IsRequired();
            e.Property(h => h.Ward).HasMaxLength(100).IsRequired();
            e.Property(h => h.District).HasMaxLength(100).IsRequired();
            e.Property(h => h.City).HasMaxLength(100).IsRequired();
            e.Property(h => h.Status).HasConversion<string>();

            e.HasMany(h => h.Orders)
             .WithOne(o => o.Hub)
             .HasForeignKey(o => o.HubId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(h => h.UserHubs)
             .WithOne(uh => uh.Hub)
             .HasForeignKey(uh => uh.HubId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserHub>(e =>
        {
            e.HasIndex(uh => new { uh.UserId, uh.HubId }).IsUnique();

            e.HasOne(uh => uh.User)
             .WithMany(u => u.UserHubs)
             .HasForeignKey(uh => uh.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderClaim>(e =>
        {
            e.Property(c => c.Status).HasConversion<string>();
            e.Property(c => c.Reason).HasMaxLength(1000).IsRequired();
            e.Property(c => c.ImageUrl).HasMaxLength(2048);

            e.HasOne(c => c.Order)
             .WithMany(o => o.Claims)
             .HasForeignKey(c => c.OrderId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Customer)
             .WithMany()
             .HasForeignKey(c => c.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(c => c.OrderId).IsUnique();
        });

        modelBuilder.Entity<HubInventory>(e =>
        {
            e.HasKey(hi => new { hi.ProductId, hi.HubId });

            e.HasOne(hi => hi.Product)
             .WithMany(p => p.HubInventories)
             .HasForeignKey(hi => hi.ProductId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(hi => hi.Hub)
             .WithMany(h => h.HubInventories)
             .HasForeignKey(hi => hi.HubId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(hi => hi.Stock).HasDefaultValue(0);
        });

        modelBuilder.Entity<Article>(e =>
        {
            e.Property(a => a.Title).HasMaxLength(200).IsRequired();
            e.Property(a => a.Excerpt).HasMaxLength(500).IsRequired();
            e.Property(a => a.Category).HasMaxLength(100).IsRequired();
            e.Property(a => a.ImageUrl).HasMaxLength(2048);
        });

        modelBuilder.Entity<User>(e =>
            e.Property(u => u.WalletBalance)
             .HasColumnType("decimal(18,2)")
             .HasDefaultValue(0m));

        modelBuilder.Entity<WalletTransaction>(e =>
        {
            e.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            e.Property(t => t.Type).HasConversion<string>();
            e.Property(t => t.Description).HasMaxLength(500).IsRequired();

            e.HasOne(t => t.User)
             .WithMany(u => u.WalletTransactions)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(t => t.Order)
             .WithMany()
             .HasForeignKey(t => t.OrderId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        modelBuilder.Entity<WalletTopupRequest>(e =>
        {
            e.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            e.Property(t => t.PaymentRef).HasMaxLength(50).IsRequired();
            e.Property(t => t.Status).HasConversion<string>();
            e.HasIndex(t => t.PaymentRef).IsUnique();

            e.HasOne(t => t.User)
             .WithMany()
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WithdrawRequest>(e =>
        {
            e.Property(w => w.Amount).HasColumnType("decimal(18,2)");
            e.Property(w => w.BankName).HasMaxLength(100).IsRequired();
            e.Property(w => w.AccountNumber).HasMaxLength(30).IsRequired();
            e.Property(w => w.HolderName).HasMaxLength(200).IsRequired();
            e.Property(w => w.Status).HasConversion<string>();
            e.Property(w => w.AdminNote).HasMaxLength(500);

            e.HasOne(w => w.User)
             .WithMany()
             .HasForeignKey(w => w.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FlashSaleSession>(e =>
            e.Property(s => s.Name).HasMaxLength(200).IsRequired());

        modelBuilder.Entity<FlashSaleItem>(e =>
        {
            e.Property(i => i.FlashSalePrice).HasColumnType("decimal(18,2)");
            e.Property(i => i.SoldCount).HasDefaultValue(0);

            e.HasIndex(i => new { i.FlashSaleSessionId, i.ProductId }).IsUnique();

            e.HasOne(i => i.Session)
             .WithMany(s => s.Items)
             .HasForeignKey(i => i.FlashSaleSessionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.Product)
             .WithMany()
             .HasForeignKey(i => i.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Warehouse>(e =>
        {
            e.Property(w => w.Name).HasMaxLength(200).IsRequired();
            e.Property(w => w.Address).HasMaxLength(500).IsRequired();
            e.Property(w => w.Ward).HasMaxLength(100).IsRequired();
            e.Property(w => w.District).HasMaxLength(100).IsRequired();
            e.Property(w => w.Province).HasMaxLength(100).IsRequired();
            e.Property(w => w.PhoneNumber).HasMaxLength(20);
            e.Property(w => w.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<OrderDamagedReport>(e =>
        {
            e.Property(r => r.RefundAmount).HasColumnType("decimal(18,2)");
            e.Property(r => r.Reason).IsRequired().HasMaxLength(1000);
            e.Property(r => r.Status).HasConversion<string>();

            e.HasOne(r => r.Order)
             .WithMany()
             .HasForeignKey(r => r.OrderId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Product)
             .WithMany()
             .HasForeignKey(r => r.ProductId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.ReportedByAgent)
             .WithMany()
             .HasForeignKey(r => r.ReportedByAgentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasOne(rt => rt.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(rt => rt.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(rt => rt.Token).HasMaxLength(500).IsRequired();
            e.HasIndex(rt => rt.Token).IsUnique();
        });

        modelBuilder.Entity<UserNotification>(e =>
        {
            e.Property(n => n.Type).HasMaxLength(50).IsRequired();
            e.Property(n => n.Title).HasMaxLength(200).IsRequired();
            e.Property(n => n.Body).HasMaxLength(1000).IsRequired();

            e.HasOne(n => n.User)
             .WithMany()
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(n => new { n.UserId, n.IsRead });
        });

        modelBuilder.Entity<Voucher>(e =>
        {
            e.Property(v => v.Code).HasMaxLength(50).IsRequired();
            e.HasIndex(v => v.Code).IsUnique();
            e.Property(v => v.Type).HasMaxLength(20).IsRequired();
            e.Property(v => v.DiscountValue).HasColumnType("decimal(18,2)");
            e.Property(v => v.MinOrderAmount).HasColumnType("decimal(18,2)");
            e.Property(v => v.UsedCount).HasDefaultValue(0);
            e.Property(v => v.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<PushToken>(e =>
        {
            e.HasOne(pt => pt.User)
             .WithMany()
             .HasForeignKey(pt => pt.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(pt => pt.Token).HasMaxLength(200).IsRequired();
            e.HasIndex(pt => pt.Token).IsUnique();
            e.Property(pt => pt.Platform).HasMaxLength(10);
        });

        modelBuilder.Entity<LoyaltyAccount>(e =>
        {
            e.HasIndex(la => la.UserId).IsUnique();
            e.Property(la => la.PointsBalance).HasDefaultValue(0);
            e.Property(la => la.TotalEarned).HasDefaultValue(0);
            e.Property(la => la.TotalRedeemed).HasDefaultValue(0);

            e.HasOne(la => la.User)
             .WithMany()
             .HasForeignKey(la => la.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoyaltyTransaction>(e =>
        {
            e.Property(lt => lt.Type).HasMaxLength(20).IsRequired();
            e.Property(lt => lt.Description).HasMaxLength(200).IsRequired();
            e.HasIndex(lt => lt.UserId);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;

        return base.SaveChangesAsync(cancellationToken);
    }
}
