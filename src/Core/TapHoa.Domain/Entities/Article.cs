namespace TapHoa.Domain.Entities;

public class Article : BaseEntity
{
    public string Title { get; set; } = "";
    public string Excerpt { get; set; } = "";
    public string Content { get; set; } = "";
    public string Category { get; set; } = "";
    public string? ImageUrl { get; set; }
    public int ReadTimeMinutes { get; set; } = 5;
    public bool IsPublished { get; set; } = true;
}
