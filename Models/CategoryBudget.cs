namespace GastaApp.Models;

/// <summary>
/// A category's budget for one specific month. This is the SOLE source of truth for
/// category budgets — Category no longer carries its own budget field. A month with
/// no row here for a given category simply has 0 budgeted; nothing carries forward
/// automatically (see ExpenseSummaryService.GetPreviousMonthBudgetsAsync for the
/// explicit "Copy from Last Month" action instead).
/// </summary>
public class CategoryBudget
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
}