using GastaApp.Data.Repositories;
using GastaApp.Models;

namespace GastaApp.Services;

public class SeedService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryBudgetRepository _categoryBudgetRepository;

    public SeedService(
        ICategoryRepository categoryRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IBudgetRepository budgetRepository,
        ICategoryBudgetRepository categoryBudgetRepository)
    {
        _categoryRepository = categoryRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _budgetRepository = budgetRepository;
        _categoryBudgetRepository = categoryBudgetRepository;
    }

    public async Task SeedIfEmptyAsync()
    {
        if ((await _categoryRepository.GetActiveOrderedAsync()).Count > 0)
            return;

        var categories = new List<Category>
        {
            // All of these were originally in the 70-84% lightness range — pastel enough
            // that white text (used directly on top of these colors in AddExpenseSheet's
            // header banner and Save button) had genuinely poor contrast. Deepened into a
            // consistent ~55-68% lightness band: still clearly reads as each category's
            // color, pops against the app's pale lavender surfaces, and gives white text
            // enough contrast to read comfortably.
            new() { Name = "Shopee", LogoImage = "shopeecat", ColorKey = "#FF7A52", SortOrder = 0 },
            new() { Name = "Lazada", LogoImage = "lazadacat", ColorKey = "#FF4FA3", SortOrder = 1 },
            new() { Name = "Grab", LogoImage = "grab", ColorKey = "#4ECD82", SortOrder = 2 },
            new() { Name = "Eat Out", LogoImage = "eatout", ColorKey = "#FFA94D", SortOrder = 3 },
            new() { Name = "Groceries", LogoImage = "groceries", ColorKey = "#E8B93E", SortOrder = 4 },
            new() { Name = "Health", LogoImage = "health", ColorKey = "#FF8FAE", SortOrder = 5 },
            new() { Name = "Transpo", LogoImage = "transpo", ColorKey = "#45C7D1", SortOrder = 6 },
            new() { Name = "Gifts", LogoImage = "gifts", ColorKey = "#A968EE", SortOrder = 7 },
        };
        foreach (var c in categories)
            await _categoryRepository.SaveAsync(c);

        var paymentMethods = new List<PaymentMethod>
        {
            new() { Name = "GCash", LogoImage = "gcash", ColorKey = "#d9daff", SortOrder = 0 },
            new() { Name = "Credit Card", LogoImage = "creditcard", ColorKey = "#fed6d7", SortOrder = 1 },
            new() { Name = "Cash", LogoImage = "cash", ColorKey = "#ceffcf", SortOrder = 2 },
            new() { Name = "Maya", LogoImage = "maya", ColorKey = "#1F1B18", SortOrder = 3 },
            new() { Name = "Debit Card", LogoImage = "debitcard", ColorKey = "#ffe0c3", SortOrder = 4 },
            new() { Name = "MariBank", LogoImage = "maribank", ColorKey = "#f8e9df", SortOrder = 5 },
            new() { Name = "GOtyme", LogoImage = "gotyme", ColorKey = "#a3eef3", SortOrder = 6 },
            new() { Name = "ShoppeePay", LogoImage = "shopeepay", ColorKey = "#ffd3cc", SortOrder = 7 },
        };
        foreach (var m in paymentMethods)
            await _paymentMethodRepository.SaveAsync(m);

        var now = DateTime.Now;

        // Overall monthly budget: 40,000 for the current month specifically — NOT a
        // recurring default. Next month starts with nothing set until the user either
        // enters a new amount or uses "Copy from Last Month" on the Manage Budget page.
        await _budgetRepository.SetForMonthAsync(now.Year, now.Month, 40000m);

        // Each category budgeted at 5,000 for the current month specifically (8
        // categories x 5,000 = 40,000, matching the overall budget exactly). Same
        // "this month only" scoping as the overall budget above — CategoryBudget is
        // the only source of truth for category budgets now, there's no fallback.
        foreach (var c in categories)
            await _categoryBudgetRepository.SetForCategoryMonthAsync(c.Id, now.Year, now.Month, 5000m);
    }
}