namespace GastaApp.Services;

/// <summary>
/// App-wide alert/confirm dialog, replacing browser confirm()/alert() with a themed
/// custom dialog. Register as a singleton and host exactly one &lt;AlertView /&gt;
/// once at the app root (e.g. MainLayout.razor) — every page/component then just
/// injects this service and awaits ShowAsync/ShowConfirmAsync.
///
/// Usage:
///   await Alert.ShowAsync("Saved successfully.");
///   var confirmed = await Alert.ShowConfirmAsync("Delete this expense?", "Confirm Delete");
/// </summary>
public class AlertService
{
    public event Action? OnChange;

    public bool IsOpen { get; private set; }
    public string Title { get; private set; } = "";
    public string Message { get; private set; } = "";
    public bool IsConfirm { get; private set; }
    public string PrimaryText { get; private set; } = "OK";
    public string SecondaryText { get; private set; } = "Cancel";

    private TaskCompletionSource<bool>? _tcs;

    /// <summary>Single-button informational alert. Resolves when dismissed.</summary>
    public Task ShowAsync(string message, string title = "", string okText = "OK")
    {
        Title = title;
        Message = message;
        IsConfirm = false;
        PrimaryText = okText;
        return Open();
    }

    /// <summary>Two-button confirm dialog. Resolves true if confirmed, false if cancelled/dismissed.</summary>
    public Task<bool> ShowConfirmAsync(string message, string title = "", string confirmText = "Confirm", string cancelText = "Cancel")
    {
        Title = title;
        Message = message;
        IsConfirm = true;
        PrimaryText = confirmText;
        SecondaryText = cancelText;
        return Open();
    }

    private Task<bool> Open()
    {
        // If something is already open, resolve it as cancelled before replacing it —
        // avoids leaking a dangling TaskCompletionSource if a caller stacks requests.
        _tcs?.TrySetResult(false);

        IsOpen = true;
        _tcs = new TaskCompletionSource<bool>();
        OnChange?.Invoke();
        return _tcs.Task;
    }

    public void Respond(bool result)
    {
        IsOpen = false;
        OnChange?.Invoke();
        _tcs?.TrySetResult(result);
        _tcs = null;
    }
}
