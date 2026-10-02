namespace GastaApp.Services;

/// <summary>
/// Coordinates the single AddExpenseSheet instance now hosted in MainLayout, so both
/// Home's tile-tap (category known) and the global FloatingAddButton (category unknown)
/// can open the same sheet without MainLayout needing a direct reference to either
/// caller. Register as Scoped in Program.cs — see note below.
/// </summary>
public class AddExpenseUiService
{
    public bool IsOpen { get; private set; }
    public SpendSummary? Category { get; private set; }

    public event Action? OnChanged;
    public event Action? OnExpenseSaved;

    public void Open(SpendSummary? category = null)
    {
        Category = category;
        IsOpen = true;
        OnChanged?.Invoke();
    }

    public void Close()
    {
        IsOpen = false;
        OnChanged?.Invoke();
    }

    public void RaiseSaved() => OnExpenseSaved?.Invoke();
}