using GastaApp;
using GastaApp.Data;
using GastaApp.Data.Repositories;
using GastaApp.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IndexedDbService>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<ExpenseSummaryService>();
builder.Services.AddScoped<SeedService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddSingleton<AlertService>();

var host = builder.Build();

// Everything below MUST finish before RunAsync() so no page's OnInitializedAsync can
// ever read the database mid-seed. This is what was missing before: SeedIfEmptyAsync()
// was registered in DI but never actually invoked anywhere, so the DB was left
// however a stray earlier run happened to leave it, and SeedIfEmptyAsync's own
// "already seeded" guard then blocked it from ever completing properly.
//
// Resolve straight from host.Services (the root provider) rather than a manually
// created child scope. Blazor WASM only really has one long-lived scope for the app's
// whole session, so a manually created-and-disposed scope tears down its own separate
// IndexedDbService instance (and the JS module reference it holds) right after this
// block — which is what threw the AsyncDisposableServiceDispose error. Resolving from
// the root keeps the same IndexedDbService instance alive for the rest of the app.
var db = host.Services.GetRequiredService<IndexedDbService>();
await db.InitializeAsync();

var seedService = host.Services.GetRequiredService<SeedService>();
await seedService.SeedIfEmptyAsync();

var themeService = host.Services.GetRequiredService<ThemeService>();
await themeService.ApplySavedAsync();

await builder.Build().RunAsync();
