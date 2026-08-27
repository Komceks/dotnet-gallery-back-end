using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gallery.Model;

// Spring: @Entity @Table(name = "author")
// EF Core uses [Table] / [Column] data annotations, or fluent config in DbContext.
[Table("author")]
public class Author
{
    // Spring: @Id @GeneratedValue(strategy = IDENTITY)
    // EF Core: identity is the default for `int`/`long` keys.
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = null!;

    // Inverse navigation. Optional but useful.
    public ICollection<Image> Images { get; set; } = new List<Image>();
}
