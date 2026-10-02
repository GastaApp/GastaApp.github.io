using GastaApp.Data.Repositories;

namespace GastaApp.Services;

public record SpendSummary(int Id, string Name, string ColorKey, string LogoImage, decimal Spent, decimal Budget, string? IconKey = null)
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
    string? Notes,
    string? CategoryIconKey = null,
    string? PaymentMethodName = null,
    string? PaymentMethodLogo = null,
    string? PaymentMethodColorKey = null,
    int? CategoryId = null,
    int? PaymentMethodId = null);

public record DailySpendPoint(DateTime Date, decimal Amount);

public record MonthComparison(decimal Current, decimal Previous)
{
    public bool IsIncrease => Current > Previous;
    public double PercentChange => Previous <= 0
        ? (Current > 0 ? 100 : 0)
        : (double)Math.Abs((Current - Previous) / Previous * 100m);
}

public record CategoryBudgetRow(int CategoryId, string Name, string ColorKey, string LogoImage, decimal Amount, string? IconKey = null);

public class ExpenseSummaryService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryBudgetRepository _categoryBudgetRepository;

    public ExpenseSummaryService(
        IExpenseRepository expenseRepository,
        ICategoryRepository categoryRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IBudgetRepository budgetRepository,
        ICategoryBudgetRepository categoryBudgetRepository)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _budgetRepository = budgetRepository;
        _categoryBudgetRepository = categoryBudgetRepository;
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
        var budgets = await _categoryBudgetRepository.GetAllForMonthAsync(year, month);

        return categories
            .Select(c =>
            {
                // No fallback — a category with no CategoryBudget row for this specific
                // month simply has 0 budgeted. Nothing carries forward automatically;
                // that's an explicit user action (see CopyBudgetsFromPreviousMonthAsync).
                var budget = budgets.FirstOrDefault(b => b.CategoryId == c.Id)?.Amount ?? 0;
                return new SpendSummary(
                    c.Id, c.Name, c.ColorKey, c.LogoImage,
                    expenses.Where(e => e.CategoryId == c.Id).Sum(e => e.Amount),
                    budget, c.IconKey);
            })
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
                    e.Id, e.Date, e.Amount,
                    category?.Name ?? "Uncategorized",
                    category?.LogoImage ?? "",
                    category?.ColorKey ?? "#9AA0A6",
                    e.Notes,
                    category?.IconKey,
                    CategoryId: e.CategoryId,
                    PaymentMethodId: e.PaymentMethodId);
            })
            .ToList();
    }

    public Task SetMonthlyBudgetAsync(int year, int month, decimal amount) =>
        _budgetRepository.SetForMonthAsync(year, month, amount);

    /// <summary>
    /// Every category with its budget for the given month (0 if nothing has been set
    /// for that specific month — no fallback default) — used by the "Set Your Monthly
    /// Budget" page's Category Budgets list.
    /// </summary>
    public async Task<List<CategoryBudgetRow>> GetCategoryBudgetRowsAsync(int year, int month)
    {
        var categories = await _categoryRepository.GetActiveOrderedAsync();
        var budgets = await _categoryBudgetRepository.GetAllForMonthAsync(year, month);

        return categories
            .Select(c =>
            {
                var amount = budgets.FirstOrDefault(b => b.CategoryId == c.Id)?.Amount ?? 0;
                return new CategoryBudgetRow(c.Id, c.Name, c.ColorKey, c.LogoImage, amount, c.IconKey);
            })
            .ToList();
    }

    /// <summary>
    /// Sets a category's budget for ONE specific month only — CategoryBudget is the
    /// sole source of truth, so this doesn't touch any other month.
    /// </summary>
    public Task SetCategoryBudgetAsync(int categoryId, int year, int month, decimal amount) =>
        _categoryBudgetRepository.SetForCategoryMonthAsync(categoryId, year, month, amount);

    /// <summary>
    /// Reads (without writing anything) the previous month's overall budget and every
    /// category's budget — used by the Manage Budget page's explicit "Copy from Last
    /// Month" action. Nothing is persisted here; the caller applies these to its local
    /// editing state and the user still has to hit Save, same as any other edit.
    /// </summary>
    public async Task<(decimal OverallBudget, List<CategoryBudgetRow> CategoryBudgets)> GetPreviousMonthBudgetsAsync(int year, int month)
    {
        var prevAnchor = new DateTime(year, month, 1).AddMonths(-1);
        var prevBudget = await _budgetRepository.GetForMonthAsync(prevAnchor.Year, prevAnchor.Month);
        var prevCategoryRows = await GetCategoryBudgetRowsAsync(prevAnchor.Year, prevAnchor.Month);
        return (prevBudget?.Amount ?? 0, prevCategoryRows);
    }

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
                    e.Id, e.Date, e.Amount,
                    category?.Name ?? "Uncategorized",
                    category?.LogoImage ?? "",
                    category?.ColorKey ?? "#9AA0A6",
                    e.Notes, category?.IconKey,
                    CategoryId: e.CategoryId,
                    PaymentMethodId: e.PaymentMethodId);
            })
            .ToList();
    }

    /// <summary>
    /// All transactions across every payment method for a given month, most recent first.
    /// Now includes PaymentMethod display fields AND both entity Ids, since this is the
    /// only page (All Transactions) that shows payment method per row and needs to
    /// filter by it.
    /// </summary>
    public async Task<List<ExpenseListItem>> GetAllTransactionsForMonthAsync(int year, int month)
    {
        var expenses = (await GetMonthExpensesAsync(year, month))
            .OrderByDescending(e => e.Date)
            .ToList();

        var categories = await _categoryRepository.GetActiveOrderedAsync();
        var paymentMethods = await _paymentMethodRepository.GetActiveOrderedAsync();

        return expenses
            .Select(e =>
            {
                var category = categories.FirstOrDefault(c => c.Id == e.CategoryId);
                var method = paymentMethods.FirstOrDefault(m => m.Id == e.PaymentMethodId);
                return new ExpenseListItem(
                    e.Id, e.Date, e.Amount,
                    category?.Name ?? "Uncategorized",
                    category?.LogoImage ?? "",
                    category?.ColorKey ?? "#9AA0A6",
                    e.Notes,
                    category?.IconKey,
                    method?.Name,
                    method?.LogoImage,
                    method?.ColorKey,
                    CategoryId: e.CategoryId,
                    PaymentMethodId: e.PaymentMethodId);
            })
            .ToList();
    }

    private async Task<List<Models.Expense>> GetMonthExpensesAsync(int year, int month)
    {
        var all = await _expenseRepository.GetAllAsync();
        return all.Where(e => e.Date.Year == year && e.Date.Month == month).ToList();
    }
}