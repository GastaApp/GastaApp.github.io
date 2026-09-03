using GastaApp.Data.Repositories;

namespace GastaApp.Services;

public record SpendSummary(int Id, string Name, string ColorKey, string LogoImage, decimal Spent, decimal Budget)
{
    public double PercentSpent => Budget <= 0 ? 0 : Math.Clamp((double)(Spent / Budget * 100m), 0, 100);
}

public record MonthSummary(decimal Budget, decimal Spent)
{
    public decimal Remaining => Budget - Spent;
    public double PercentSpent => Budget <= 0 ? 0 : Math.Clamp((double)(Spent / Budget), 0, 1);
}

public record ExpenseListItem(
    int Id,
    DateTime Date,
    decimal Amount,
    string CategoryName,
    string CategoryLogo,
    string CategoryColorKey,
    string? Notes);

public record DailySpendPoint(DateTime Date, decimal Amount);

public record MonthComparison(decimal Current, decimal Previous)
{
    public bool IsIncrease => Current > Previous;
    public double PercentChange => Previous <= 0
        ? (Current > 0 ? 100 : 0)
        : (double)Math.Abs((Current - Previous) / Previous * 100m);
}

public class ExpenseSummaryService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IBudgetRepository _budgetRepository;

    public ExpenseSummaryService(
        IExpenseRepository expenseRepository,
        ICategoryRepository categoryRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IBudgetRepository budgetRepository)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _budgetRepository = budgetRepository;
    }

    public async Task<MonthSummary> GetMonthSummaryAsync(int year, int month)
    {
        var budget = await _budgetRepository.GetForMonthAsync(year, month);
        var spent = await GetMonthExpensesAsync(year, month);
        return new MonthSummary(budget?.Amount ?? 0, spent.Sum(e => e.Amount));
    }

    public async Task<List<SpendSummary>> GetCategorySummariesAsync(int year, int month)
    {
        var categories = await _categoryRepository.GetActiveOrderedAsync();
        var expenses = await GetMonthExpensesAsync(year, month);

        return categories
            .Select(c => new SpendSummary(
                c.Id, c.Name, c.ColorKey, c.LogoImage,
                expenses.Where(e => e.CategoryId == c.Id).Sum(e => e.Amount),
                c.MonthlyBudget))
            .ToList();
    }

    public async Task<List<SpendSummary>> GetPaymentMethodSummariesAsync(int year, int month)
    {
        var methods = await _paymentMethodRepository.GetActiveOrderedAsync();
        var expenses = await GetMonthExpensesAsync(year, month);

        return methods
            .Select(m => new SpendSummary(
                m.Id, m.Name, m.ColorKey, m.LogoImage,
                expenses.Where(e => e.PaymentMethodId == m.Id).Sum(e => e.Amount),
                Budget: 0))
            .ToList();
    }

    /// <summary>
    /// Transaction line items for a single payment method, current-month scoped like the
    /// rest of this service. Used by the per-payment-method Expense List page.
    /// </summary>
    public async Task<List<ExpenseListItem>> GetTransactionsForPaymentMethodAsync(int year, int month, int paymentMethodId)
    {
        var expenses = (await GetMonthExpensesAsync(year, month))
            .Where(e => e.PaymentMethodId == paymentMethodId)
            .OrderByDescending(e => e.Date)
            .ToList();

        var categories = await _categoryRepository.GetActiveOrderedAsync();

        return expenses
            .Select(e =>
            {
                var category = categories.FirstOrDefault(c => c.Id == e.CategoryId);
                return new ExpenseListItem(
                    e.Id,
                    e.Date,
                    e.Amount,
                    category?.Name ?? "Uncategorized",
                    category?.LogoImage ?? "",
                    category?.ColorKey ?? "#9AA0A6",
                    e.Notes);
            })
            .ToList();
    }

    public Task SetMonthlyBudgetAsync(int year, int month, decimal amount) =>
        _budgetRepository.SetForMonthAsync(year, month, amount);

    /// <summary>Current month's total vs the previous month's total, for the Stats page comparison badge.</summary>
    public async Task<MonthComparison> GetMonthOverMonthAsync(int year, int month)
    {
        var current = (await GetMonthExpensesAsync(year, month)).Sum(e => e.Amount);
        var prevAnchor = new DateTime(year, month, 1).AddMonths(-1);
        var previous = (await GetMonthExpensesAsync(prevAnchor.Year, prevAnchor.Month)).Sum(e => e.Amount);
        return new MonthComparison(current, previous);
    }

    /// <summary>Daily totals for the last 7 days (oldest first, today last), across all payment methods.</summary>
    public async Task<List<DailySpendPoint>> GetLast7DaysAsync()
    {
        var all = await _expenseRepository.GetAllAsync();
        var today = DateTime.Today;

        return Enumerable.Range(0, 7)
            .Select(i => today.AddDays(-6 + i))
            .Select(day => new DailySpendPoint(day, all.Where(e => e.Date.Date == day).Sum(e => e.Amount)))
            .ToList();
    }

    /// <summary>
    /// Most recent transactions across ALL payment methods and months (not scoped like the
    /// rest of this service) — used for the Stats page's Recent Transactions list.
    /// </summary>
    public async Task<List<ExpenseListItem>> GetRecentTransactionsAsync(int take = 5)
    {
        var all = await _expenseRepository.GetAllAsync();
        var categories = await _categoryRepository.GetActiveOrderedAsync();

        return all
            .OrderByDescending(e => e.Date)
            .Take(take)
            .Select(e =>
            {
                var category = categories.FirstOrDefault(c => c.Id == e.CategoryId);
                return new ExpenseListItem(
                    e.Id,
                    e.Date,
                    e.Amount,
                    category?.Name ?? "Uncategorized",
                    category?.LogoImage ?? "",
                    category?.ColorKey ?? "#9AA0A6",
                    e.Notes);
            })
            .ToList();
    }

    /// <summary>
    /// All transactions across every payment method for a given month, most recent first.
    /// Used by the full All Transactions page (with its own month switcher), as opposed to
    /// GetRecentTransactionsAsync (count-limited, ignores month) or
    /// GetTransactionsForPaymentMethodAsync (single payment method).
    /// </summary>
    public async Task<List<ExpenseListItem>> GetAllTransactionsForMonthAsync(int year, int month)
    {
        var expenses = (await GetMonthExpensesAsync(year, month))
            .OrderByDescending(e => e.Date)
            .ToList();

        var categories = await _categoryRepository.GetActiveOrderedAsync();

        return expenses
            .Select(e =>
            {
                var category = categories.FirstOrDefault(c => c.Id == e.CategoryId);
                return new ExpenseListItem(
                    e.Id,
                    e.Date,
                    e.Amount,
                    category?.Name ?? "Uncategorized",
                    category?.LogoImage ?? "",
                    category?.ColorKey ?? "#9AA0A6",
                    e.Notes);
            })
            .ToList();
    }

    private async Task<List<Models.Expense>> GetMonthExpensesAsync(int year, int month)
    {
        var all = await _expenseRepository.GetAllAsync();
        return all.Where(e => e.Date.Year == year && e.Date.Month == month).ToList();
    }
}
