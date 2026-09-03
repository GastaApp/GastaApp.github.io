namespace GastaApp.Models;

public class PaymentMethod
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoImage { get; set; } = string.Empty;
    public string ColorKey { get; set; } = "#5B7FE8";
    public int SortOrder { get; set; }
}
