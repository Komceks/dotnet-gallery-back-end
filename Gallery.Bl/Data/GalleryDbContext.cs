using Gallery.Model;
using Microsoft.EntityFrameworkCore;

namespace Gallery.Bl.Data;

// DbContext = the EF Core unit-of-work + repository container.
// Mental map: roughly Spring's EntityManager + JpaRepository combined.
// You inject this into services (constructor injection, same idea as @Autowired).
public class GalleryDbContext : DbContext
{
    public GalleryDbContext(DbContextOptions<GalleryDbContext> options) : base(options) { }

    // Each DbSet<T> is like JpaRepository<T, ?> — your access point for that entity.
    public DbSet<Image> Images => Set<Image>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Spring's @JoinTable(name="images_tags", joinColumns=image_id, inverseJoinColumns=tag_id)
        // is configured here with the Fluent API. This is the EF Core equivalent of orm.xml /
        // entity annotations for the relationship details.
        modelBuilder.Entity<Image>()
            .HasMany(i => i.Tags)
            .WithMany(t => t.Images)
            .UsingEntity(j => j.ToTable("images_tags"));

        modelBuilder.Entity<Image>()
            .HasOne(i => i.Author)
            .WithMany(a => a.Images)
            .HasForeignKey(i => i.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraints — equivalent to setting unique=true on @Column.
        modelBuilder.Entity<Author>()
            .HasIndex(a => a.Name)
            .IsUnique();

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
