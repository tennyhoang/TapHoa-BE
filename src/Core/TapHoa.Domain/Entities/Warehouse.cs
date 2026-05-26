namespace TapHoa.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Name        { get; set; } = default!;
    public string Address     { get; set; } = default!;
    public string Ward        { get; set; } = default!;
    public string District    { get; set; } = default!;
    public string Province    { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public bool   IsActive    { get; set; } = true;
}
