using GastaApp.Models;

namespace GastaApp.Data.Repositories;

public interface IBudgetRepository
{
    Task<Budget?> GetForMonthAsync(int year, int month);
    Task SetForMonthAsync(int year, int month, decimal amount);
}

public class BudgetRepository : IBudgetRepository
{
    private const string Store = "budgets";
    private readonly IndexedDbService _db;

    public BudgetRepository(IndexedDbService db) => _db = db;

    public async Task<Budget?> GetForMonthAsync(int year, int month) =>
        (await _db.GetAllAsync<Budget>(Store)).FirstOrDefault(b => b.Year == year && b.Month == month);

    public async Task SetForMonthAsync(int year, int month, decimal amount)
    {
        var existing = await GetForMonthAsync(year, month);
        var budget = existing ?? new Budget { Year = year, Month = month };
        budget.Amount = amount;
        var id = await _db.PutAsync(Store, budget);
        budget.Id = id; // put() now returns the real autoIncrement key when Id was 0
    }
}