using Microsoft.EntityFrameworkCore;
using ProductsService.Data.Entities;

namespace ProductsService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ProductEntity> Products { get; set; }
        public DbSet<GroupResultEntity> GroupResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductEntity>(entity =>
            {
                entity.ToTable("Products");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Unit)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PricePerUnit)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Quantity)
                    .IsRequired();

                entity.Property(e => e.IsProcessed)
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<GroupResultEntity>(entity =>
            {
                entity.ToTable("GroupResults");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.TotalPrice)
                      .IsRequired()
                      .HasColumnType("decimal(18, 2)");

                entity.HasMany(e => e.Products)
                      .WithOne(e => e.GroupResult)
                      .HasForeignKey(e => e.GroupResultId);
            });
        }
    }
}
