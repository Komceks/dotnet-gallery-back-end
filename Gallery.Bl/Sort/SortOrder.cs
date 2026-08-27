namespace Gallery.Bl.Sort;

// Spring: enum SortOrder { ASC, DESC }
// In .NET, the value names match the wire format ("ASC", "DESC")
// thanks to JsonStringEnumConverter (configured in Program.cs).
public enum SortOrder
{
    ASC,
    DESC
}
