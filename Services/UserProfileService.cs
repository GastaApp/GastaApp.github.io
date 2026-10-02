using Microsoft.JSInterop;

namespace GastaApp.Services;

/// <summary>
/// Owns the user's nickname and first-run "onboarded" flag, both persisted to
/// localStorage — same JS-interop convention Settings.razor already uses for
/// BudgetReminders. Scoped so the in-memory cache is shared across the app session
/// without re-hitting localStorage on every read.
/// </summary>
public class UserProfileService
{
    private const string NicknameKey = "gasta-nickname";
    private const string OnboardedKey = "gasta-onboarded";

    private readonly IJSRuntime _js;
    private string? _nickname;
    private bool _nicknameLoaded;

    public UserProfileService(IJSRuntime js) => _js = js;

    public async Task<string> GetNicknameAsync()
    {
        if (!_nicknameLoaded)
        {
            _nickname = await SafeGetItem(NicknameKey);
            _nicknameLoaded = true;
        }
        return string.IsNullOrWhiteSpace(_nickname) ? "Friend" : _nickname!;
    }

    public async Task SetNicknameAsync(string nickname)
    {
        _nickname = nickname.Trim();
        _nicknameLoaded = true;
        await _js.InvokeVoidAsync("localStorage.setItem", NicknameKey, _nickname);
    }

    public async Task<bool> IsOnboardedAsync() =>
        await SafeGetItem(OnboardedKey) == "true";

    public async Task MarkOnboardedAsync() =>
        await _js.InvokeVoidAsync("localStorage.setItem", OnboardedKey, "true");

    /// <summary>
    /// Wipes nickname + onboarded flag, so a fresh Reset All Data means a genuinely
    /// fresh start — the person sees onboarding again next launch, not just an empty
    /// database with their old name still showing on Home.
    /// </summary>
    public async Task ClearProfileAsync()
    {
        _nickname = null;
        _nicknameLoaded = false;
        await _js.InvokeVoidAsync("localStorage.removeItem", NicknameKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", OnboardedKey);
    }

    private async Task<string?> SafeGetItem(string key)
    {
        try { return await _js.InvokeAsync<string?>("localStorage.getItem", key); }
        catch { return null; }
    }
}