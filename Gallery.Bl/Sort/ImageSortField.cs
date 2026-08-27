namespace Gallery.Bl.Sort;

// Spring's ImageSortEnum stored a Supplier<SingularAttribute<Image, ?>> to the JPA metamodel.
// In .NET with EF Core, we don't need metamodel suppliers — we use lambda expressions directly
// in the service (see ImageService.Search). This enum just names the sortable fields.
//
// The names map to the wire format your Angular sends: "UPLOAD_DATE", "NAME", "DATE".
public enum ImageSortField
{
    UPLOAD_DATE,
    NAME,
    DATE
}
