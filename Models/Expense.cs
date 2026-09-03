namespace GastaApp.Models;

public class Expense
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public int PaymentMethodId { get; set; }
    public string? Notes { get; set; }
}
