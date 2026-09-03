using GastaApp.Models;

namespace GastaApp.Data.Repositories;

public interface IPaymentMethodRepository
{
    Task<List<PaymentMethod>> GetActiveOrderedAsync();
    Task<PaymentMethod?> GetByIdAsync(int id);
    Task<int> SaveAsync(PaymentMethod method);
}

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private const string Store = "paymentMethods";
    private readonly IndexedDbService _db;

    public PaymentMethodRepository(IndexedDbService db) => _db = db;

    public async Task<List<PaymentMethod>> GetActiveOrderedAsync() =>
        (await _db.GetAllAsync<PaymentMethod>(Store)).OrderBy(p => p.SortOrder).ToList();

    public async Task<PaymentMethod?> GetByIdAsync(int id) =>
        (await _db.GetAllAsync<PaymentMethod>(Store)).FirstOrDefault(p => p.Id == id);

    public async Task<int> SaveAsync(PaymentMethod method)
    {
        var id = await _db.PutAsync(Store, method);
        method.Id = id; // put() now returns the real autoIncrement key when Id was 0
        return id;
    }
}