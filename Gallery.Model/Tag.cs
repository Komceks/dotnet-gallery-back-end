using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gallery.Model;

[Table("tag")]
public class Tag
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = null!;

    // Spring inverse side: @ManyToMany(mappedBy = "tags")
    // In EF Core, both sides have ICollection<T>; the join table is configured in DbContext.
    public ICollection<Image> Images { get; set; } = new HashSet<Image>();
}
