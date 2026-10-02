using Microsoft.JSInterop;

namespace GastaApp.Services;

public record AccentPreset(
    string Id, string Name, string SwatchHex,
    string LightPrimary, string LightPrimaryDark, string LightNavActive,
    string LightBg, string LightSurfaceAlt, string LightBorder,
    string DarkPrimary, string DarkPrimaryDark, string DarkNavActive);

/// <summary>
/// Runtime accent-color switching. Works because every component in theme.css reads
/// color exclusively through CSS custom properties — this injects a small stylesheet
/// redefining the relevant variables for BOTH the light (:root) and dark
/// ([data-theme="dark"]) selectors, same shape as theme.css itself.
///
/// --color-primary/--color-primary-dark/--nav-active-bg always shift per preset, in
/// BOTH themes. Light-mode background/surface-alt/border/nav-bg ALSO shift per
/// preset, but only when BackgroundTintEnabled is on (opt-in, off by default) — see
/// AccentColorSheet. Dark mode backgrounds are NEVER per-preset, regardless of this
/// toggle: modern dark UIs (Claude, GitHub, Linear) use one neutral dark base
/// regardless of accent, since a tinted dark background fighting an accent color
/// reads muddy rather than premium.
/// </summary>
public class AccentColorService
{
    private const string AccentIdKey = "gasta-accent-id";
    private const string BgTintKey = "gasta-accent-bg-tint";
    private readonly IJSRuntime _js;

    public AccentColorService(IJSRuntime js) => _js = js;

    // Light neutral triads are hand-picked to match the same subtlety/lightness as
    // the original purple values (bg ~97% lightness, surface-alt ~94%, border ~88%,
    // low saturation) — estimates, not pixel-verified against a live render.
    public static readonly List<AccentPreset> Presets = new()
    {
        new("purple", "Purple", "#5B4FE8",
            "#5B4FE8", "#4A3FC4", "#C9C2F5", "#F7F6FC", "#EFEDFA", "#E3E0F5",
            "#7C6FFF", "#6355E0", "#453E7A"),
        new("blue", "Blue", "#3B6FEA",
            "#3B6FEA", "#2F58C4", "#C2D3F5", "#F5F7FD", "#EAF0FB", "#D8E3F7",
            "#4C82E0", "#3A68C0", "#324B78"),
        new("teal", "Teal", "#12A594",
            "#12A594", "#0D8577", "#B7E8E1", "#F2FAF8", "#E3F5F1", "#CDEDE6",
            "#26B39C", "#1D8F7D", "#1F5C52"),
        new("green", "Green", "#1F8A4C",
            "#1F8A4C", "#176B3B", "#BFE3C9", "#F5FAF6", "#E8F5EC", "#D3EBDA",
            "#2EA866", "#228053", "#2F5C40"),
        new("coral", "Coral", "#E85B4F",
            "#E85B4F", "#C43F35", "#F5C9C2", "#FDF6F5", "#FBEAE8", "#F6D5D1",
            "#D45E4F", "#B84A3D", "#7A3E3E"),
        new("rose", "Rose", "#E84F91",
            "#E84F91", "#C43575", "#F5C2DD", "#FDF5F9", "#FBE9F1", "#F6D2E3",
            "#E0568F", "#C43F73", "#7A3E5C"),
        new("amber", "Amber", "#A87A15",
            "#A87A15", "#8A6410", "#F0DDB0", "#FAF7EE", "#F3EBD6", "#E8DAB3",
            "#B3812A", "#93691F", "#6B5320"),
        new("gold", "Gold", "#B8850F",
            "#B8850F", "#936A0C", "#F3E3B4", "#FBF4E4", "#F6E8C4", "#ECD89A",
            "#B98A0F", "#93690C", "#5C4515"),
        new("slate", "Slate", "#5B6B8C",
            "#5B6B8C", "#45526B", "#C9D2E0", "#F5F6F9", "#EAEDF2", "#D8DCE6",
            "#6E82AC", "#556694", "#3A4356"),
    };

    public async Task<string> GetCurrentPresetIdAsync()
    {
        var saved = await SafeGetItem(AccentIdKey);
        return Presets.Any(p => p.Id == saved) ? saved! : "purple";
    }

    // Off by default — existing installs keep their current fixed purple-tinted
    // background until someone explicitly opts in via AccentColorSheet's toggle.
    public async Task<bool> GetBackgroundTintEnabledAsync() =>
        await SafeGetItem(BgTintKey) == "true";

    public async Task SetBackgroundTintEnabledAsync(bool enabled)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", BgTintKey, enabled ? "true" : "false");
        await ApplyInternal(await GetCurrentPresetIdAsync(), enabled);
    }

    public async Task ApplyCurrentAsync() =>
        await ApplyInternal(await GetCurrentPresetIdAsync(), await GetBackgroundTintEnabledAsync());

    public async Task SetAsync(string presetId)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", AccentIdKey, presetId);
        await ApplyInternal(presetId, await GetBackgroundTintEnabledAsync());
    }

    private async Task ApplyInternal(string presetId, bool bgTintEnabled)
    {
        var preset = Presets.FirstOrDefault(p => p.Id == presetId) ?? Presets[0];

        var lightVars = $"--color-primary:{preset.LightPrimary}; --color-primary-dark:{preset.LightPrimaryDark}; --nav-active-bg:{preset.LightNavActive};";
        if (bgTintEnabled)
        {
            lightVars += $" --bg:{preset.LightBg}; --surface-alt:{preset.LightSurfaceAlt}; --border:{preset.LightBorder}; --nav-bg:{preset.LightSurfaceAlt};";
        }

        // :root:not([data-theme="dark"]) rather than plain :root — this rule CANNOT
        // match at all while dark mode is active, regardless of CSS source order or
        // specificity tie-breaking. A plain :root here previously tied in specificity
        // with theme.css's own [data-theme="dark"] block, and since this stylesheet is
        // injected (and thus always later in the DOM) it won that tie-break — meaning
        // light-mode background values leaked into dark mode whenever background
        // tinting was turned on. This selector closes that off structurally rather
        // than relying on source order staying in our favor.
        var css =
            $":root:not([data-theme=\"dark\"]) {{ {lightVars} }}\n" +
            $"[data-theme=\"dark\"] {{ --color-primary:{preset.DarkPrimary}; --color-primary-dark:{preset.DarkPrimaryDark}; --nav-active-bg:{preset.DarkNavActive}; }}";

        await _js.InvokeVoidAsync("gastaAccent.apply", css);
    }

    private async Task<string?> SafeGetItem(string key)
    {
        try { return await _js.InvokeAsync<string?>("localStorage.getItem", key); }
        catch { return null; }
    }

    /// <summary>
    /// Wipes both the chosen accent and the background-tint preference, then
    /// re-applies the true defaults (Purple, tint off) to the live document — used by
    /// Settings' Reset All Data so a reset is genuinely a fresh start, not just a
    /// wiped database with the old theme choices still active.
    /// </summary>
    public async Task ResetToDefaultAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", AccentIdKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", BgTintKey);
        await ApplyInternal(Presets[0].Id, false);
    }
}