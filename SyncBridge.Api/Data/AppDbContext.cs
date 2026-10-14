using Microsoft.EntityFrameworkCore;
using SyncBridge.Api.Models;

namespace SyncBridge.Api.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options)
	{
	}

	public DbSet<Order> Orders => Set<Order>();

	public DbSet<OrderSyncAttempt> OrderSyncAttempts => Set<OrderSyncAttempt>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder); 

		modelBuilder.Entity<Order>()
			.HasIndex(order => order.ExternalId)
			.IsUnique();

		modelBuilder.Entity<Order>()
			.Property(order => order.TotalAmount)
			.HasPrecision(18, 2);
	}
}