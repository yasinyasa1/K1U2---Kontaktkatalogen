namespace ContactCatalog.Models;

public class Contact
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();

    public override string ToString()
    {
        return $"[{Id}] {Name} - {Email} | Tags: {string.Join(", ", Tags)}";
    }
}

