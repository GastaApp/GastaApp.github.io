using GastaApp.Models;

namespace GastaApp.Data.Repositories;

public interface ICategoryBudgetRepository
{
    Task<List<CategoryBudget>> GetAllForMonthAsync(int year, int month);
    Task<CategoryBudget?> GetForCategoryMonthAsync(int categoryId, int year, int month);
    Task SetForCategoryMonthAsync(int categoryId, int year, int month, decimal amount);
    /// <summary>Every category budget ever set, across all months and categories.
    /// Used by the Data & Backup export for full budget history.</summary>
    Task<List<CategoryBudget>> GetAllAsync();
}

public class CategoryBudgetRepository : ICategoryBudgetRepository
{
    private const string Store = "categoryBudgets";
    private readonly IndexedDbService _db;

    public CategoryBudgetRepository(IndexedDbService db) => _db = db;

    public async Task<List<CategoryBudget>> GetAllForMonthAsync(int year, int month) =>
        (await _db.GetAllAsync<CategoryBudget>(Store))
            .Where(b => b.Year == year && b.Month == month)
            .ToList();

    public async Task<CategoryBudget?> GetForCategoryMonthAsync(int categoryId, int year, int month) =>
        (await _db.GetAllAsync<CategoryBudget>(Store))
            .FirstOrDefault(b => b.CategoryId == categoryId && b.Year == year && b.Month == month);

    public async Task SetForCategoryMonthAsync(int categoryId, int year, int month, decimal amount)
    {
        var existing = await GetForCategoryMonthAsync(categoryId, year, month);
        var budget = existing ?? new CategoryBudget { CategoryId = categoryId, Year = year, Month = month };
        budget.Amount = amount;
        var id = await _db.PutAsync(Store, budget);
        budget.Id = id;
    }

    public async Task<List<CategoryBudget>> GetAllAsync() =>
        (await _db.GetAllAsync<CategoryBudget>(Store)).ToList();
}