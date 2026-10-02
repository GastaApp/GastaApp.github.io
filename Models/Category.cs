namespace GastaApp.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Legacy: filename (no extension) under wwwroot/images/categories/, used by the
    // branded seed categories (Shopee, Lazada, etc.) that have real logo assets.
    public string LogoImage { get; set; } = string.Empty;

    // New: key into CategoryIconSet.Icons, used by user-created/edited categories
    // that don't have a matching branded image asset. When set, takes priority over
    // LogoImage for rendering — see the CategoryIcon component.
    public string? IconKey { get; set; }

    public string ColorKey { get; set; } = "#E08A3C";
    public int SortOrder { get; set; }
}