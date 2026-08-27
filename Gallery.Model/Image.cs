using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gallery.Model;

// Spring: @Entity @Table(name = "image"), columns mapped via @Column.
// The Java version uses LocalDate / LocalDateTime; in .NET we use DateOnly / DateTime.
[Table("image")]
public class Image
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("image_blob")]
    public byte[] ImageBlob { get; set; } = null!;

    [Required]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Required]
    [Column("description")]
    public string Description { get; set; } = null!;

    [Required]
    [Column("date")]
    public DateOnly Date { get; set; }

    // Spring: @ManyToOne(cascade = PERSIST) @JoinColumn(name = "author_id")
    // EF Core: foreign key + navigation property. AuthorId is the FK column.
    [Column("author_id")]
    public long AuthorId { get; set; }
    public Author Author { get; set; } = null!;

    // Spring: @ManyToMany via images_tags join table.
    // Configured in GalleryDbContext.OnModelCreating.
    public ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();

    [Required]
    [Column("timestamp")]
    public DateTime UploadDate { get; set; }

    [Required]
    [Column("thumbnail")]
    public byte[] Thumbnail { get; set; } = null!;
}
