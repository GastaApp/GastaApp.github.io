using GastaApp.Models;

namespace GastaApp.Data.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetActiveOrderedAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<int> SaveAsync(Category category);
    Task DeleteAsync(int id);
}

public class CategoryRepository : ICategoryRepository
{
    private const string Store = "categories";
    private readonly IndexedDbService _db;

    public CategoryRepository(IndexedDbService db) => _db = db;

    public async Task<List<Category>> GetActiveOrderedAsync() =>
        (await _db.GetAllAsync<Category>(Store)).OrderBy(c => c.SortOrder).ToList();

    public async Task<Category?> GetByIdAsync(int id) =>
        (await _db.GetAllAsync<Category>(Store)).FirstOrDefault(c => c.Id == id);

    public async Task<int> SaveAsync(Category category)
    {
        var id = await _db.PutAsync(Store, category);
        category.Id = id; // put() now returns the real autoIncrement key when Id was 0
        return id;
    }

    public Task DeleteAsync(int id) => _db.DeleteAsync(Store, id);
}