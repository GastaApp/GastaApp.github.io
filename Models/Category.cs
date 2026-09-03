namespace GastaApp.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoImage { get; set; } = string.Empty;
    public string ColorKey { get; set; } = "#E08A3C";
    public decimal MonthlyBudget { get; set; }
    public int SortOrder { get; set; }
}
