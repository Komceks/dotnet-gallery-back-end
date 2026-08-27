namespace Gallery.Model;

// Spring's @Data Greeting POJO becomes a C# record.
// `record` gives you immutable, value-based equality, and a primary constructor — no Lombok needed.
public record Greeting(long Id, string Content, string Timestamp);
