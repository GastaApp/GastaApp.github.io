using GastaApp.Data.Repositories;
using GastaApp.Models;

namespace GastaApp.Services;

public class SeedService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IBudgetRepository _budgetRepository;

    public SeedService(
        ICategoryRepository categoryRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IBudgetRepository budgetRepository)
    {
        _categoryRepository = categoryRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _budgetRepository = budgetRepository;
    }

    public async Task SeedIfEmptyAsync()
    {
        if ((await _categoryRepository.GetActiveOrderedAsync()).Count > 0)
            return;

        var categories = new List<Category>
        {
            new() { Name = "Shoppee", LogoImage = "shopeecat", ColorKey = "#FF9576", MonthlyBudget = 5000, SortOrder = 0 },
            new() { Name = "Lazada", LogoImage = "lazadacat", ColorKey = "#FF8BC5", MonthlyBudget = 5000, SortOrder = 1 },
            new() { Name = "Grab", LogoImage = "grab", ColorKey = "#64E195", MonthlyBudget = 5000, SortOrder = 2 },
            new() { Name = "Eat Out", LogoImage = "eatout", ColorKey = "#FFCD9D", MonthlyBudget = 5000, SortOrder = 3 },
            new() { Name = "Groceries", LogoImage = "groceries", ColorKey = "#FFD66B", MonthlyBudget = 5000, SortOrder = 4 },
            new() { Name = "Health", LogoImage = "health", ColorKey = "#FFAED8", MonthlyBudget = 5000, SortOrder = 5 },
            new() { Name = "Transpo", LogoImage = "transpo", ColorKey = "#7EDAE0", MonthlyBudget = 5000, SortOrder = 6 },
            new() { Name = "Gifts", LogoImage = "gifts", ColorKey = "#E7D2FE", MonthlyBudget = 5000, SortOrder = 7 },
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
            new() { Name = "GOtyme bank", LogoImage = "gotyme", ColorKey = "#a3eef3", SortOrder = 6 },
            new() { Name = "Shoppee Pay", LogoImage = "shopeepay", ColorKey = "#ffd3cc", SortOrder = 7 },
        };
        foreach (var m in paymentMethods)
            await _paymentMethodRepository.SaveAsync(m);

        var now = DateTime.Now;
        await _budgetRepository.SetForMonthAsync(now.Year, now.Month, 5000.0m);
    }
}