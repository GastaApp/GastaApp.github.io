using GastaApp.Models;

namespace GastaApp.Data.Repositories;

public interface IExpenseRepository
{
    Task<List<Expense>> GetAllAsync();
    Task<Expense?> GetByIdAsync(int id);
    Task<int> SaveAsync(Expense expense);
    Task DeleteAsync(int id);
}

public class ExpenseRepository : IExpenseRepository
{
    private const string Store = "expenses";
    private readonly IndexedDbService _db;

    public ExpenseRepository(IndexedDbService db) => _db = db;

    public Task<List<Expense>> GetAllAsync() => _db.GetAllAsync<Expense>(Store);

    public async Task<Expense?> GetByIdAsync(int id) =>
        (await GetAllAsync()).FirstOrDefault(e => e.Id == id);

    public async Task<int> SaveAsync(Expense expense)
    {
        var id = await _db.PutAsync(Store, expense);
        expense.Id = id; // put() now returns the real autoIncrement key when Id was 0
        return id;
    }

    public Task DeleteAsync(int id) => _db.DeleteAsync(Store, id);
}