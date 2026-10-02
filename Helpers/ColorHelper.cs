namespace GastaApp.Helpers;

/// <summary>
/// Derives lightened/darkened tones from a hex color — the Blazor equivalent of the
/// MAUI build's ColorKeyConverter "Bg" parameter. Same blend math, just called
/// directly from markup instead of through an XAML converter.
///
/// Usage: style="stroke:@ColorHelper.Lighten(category.ColorKey)"
/// </summary>
public static class ColorHelper
{
    private const double DefaultBlendAmount = 0.56;
    private const double DefaultDarkTrackAmount = 0.35;

    // The app's actual dark-mode page background. Blending a category color toward
    // PURE BLACK (the old default) ignores that #191919 is a dark navy-PURPLE, not
    // neutral black — a track blended toward black drifts toward a slightly
    // mismatched, cooler/grayer hue than the page it actually sits on. Blending
    // toward this instead means a strongly-blended track genuinely melts into the
    // page, rather than approaching a different dark tone next to it.
    // MUST be kept in sync with --bg in your CSS by hand — C# can't read a CSS
    // custom property, so if --bg ever changes, update this constant to match.
    public const string DarkPageBackground = "#191919";

    public static string Lighten(string hex, double amount = DefaultBlendAmount) =>
        Blend(hex, amount, "#FFFFFF");

    public static string Darken(string hex, double amount = 0.3) =>
        Blend(hex, amount, "#000000");

    /// <summary>
    /// Theme-aware derived tone for backgrounds/tracks — lightens toward white in
    /// light mode, darkens toward BLACK in dark mode. Kept exactly as before for
    /// every existing caller (fill circles, chip backgrounds, etc.) — unchanged
    /// behavior, still useful wherever blending toward true black/white is correct.
    /// </summary>
    public static string AdaptiveTrack(
        string hex, bool isDark,
        double lightAmount = DefaultBlendAmount, double darkAmount = DefaultDarkTrackAmount) =>
        isDark ? Darken(hex, darkAmount) : Lighten(hex, lightAmount);

    /// <summary>
    /// Same idea as AdaptiveTrack, but the dark-mode side blends toward an actual
    /// target color (your page background) instead of pure black — use this
    /// specifically for ring TRACKS or anything meant to visually recede into the
    /// page in dark mode. Light mode is unchanged (still blends toward white).
    /// </summary>
    public static string AdaptiveTrackToward(
        string hex, bool isDark, string darkTarget,
        double lightAmount = DefaultBlendAmount, double darkAmount = DefaultDarkTrackAmount) =>
        isDark ? Blend(hex, darkAmount, darkTarget) : Lighten(hex, lightAmount);

    private static string Blend(string hex, double amount, string targetHex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return "#9AA0A6";

        var (r, g, b) = ParseHex(hex);
        if (r is null) return "#" + hex.TrimStart('#');

        var (tr, tg, tb) = ParseHex(targetHex);
        var targetR = tr ?? 0;
        var targetG = tg ?? 0;
        var targetB = tb ?? 0;

        var nr = Clamp((int)(r.Value + (targetR - r.Value) * amount));
        var ng = Clamp((int)(g!.Value + (targetG - g.Value) * amount));
        var nb = Clamp((int)(b!.Value + (targetB - b.Value) * amount));

        return $"#{nr:X2}{ng:X2}{nb:X2}";
    }

    private static (int? R, int? G, int? B) ParseHex(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length != 6 ||
            !int.TryParse(hex[..2], System.Globalization.NumberStyles.HexNumber, null, out var r) ||
            !int.TryParse(hex[2..4], System.Globalization.NumberStyles.HexNumber, null, out var g) ||
            !int.TryParse(hex[4..6], System.Globalization.NumberStyles.HexNumber, null, out var b))
        {
            return (null, null, null);
        }
        return (r, g, b);
    }

    private static int Clamp(int v) => Math.Clamp(v, 0, 255);

    /// <summary>
    /// Returns "black" or "white" — whichever reads better on top of the given
    /// background hex, based on actual perceived luminance (standard YIQ formula).
    /// Replaces manual per-name checks like `name == "Maya" ? "white" : "black"`,
    /// which only ever covers colors someone happened to test against — this works
    /// correctly for any hex, including future user-picked payment method colors.
    /// </summary>
    public static string ReadableTextColor(string hex)
    {
        var (r, g, b) = ParseHex(hex);
        if (r is null) return "black";

        var luminance = (r.Value * 299 + g!.Value * 587 + b!.Value * 114) / 1000.0;
        return luminance >= 140 ? "black" : "white";
    }
}

