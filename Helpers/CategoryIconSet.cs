namespace GastaApp.Helpers;

/// <summary>
/// Curated icon set for category icons — same visual language as the bottom nav's
/// inline SVG icons (stroke=currentColor, 24x24 viewBox, 2px stroke, round caps).
/// Each entry is the INNER markup only; CategoryIcon.razor wraps it in the shared
/// &lt;svg&gt; element with common attributes.
///
/// Hand-authored without a live preview — reasonable approximations of each concept,
/// not from a professional icon library. Some may need small coordinate tweaks once
/// actually seen rendered.
/// </summary>
public static class CategoryIconSet
{
    public static readonly Dictionary<string, string> Icons = new()
    {
        ["shopping-bag"] = "<path d=\"M6 2 3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4Z\"/><path d=\"M3 6h18\"/><path d=\"M16 10a4 4 0 0 1-8 0\"/>",
        ["shopping-cart"] = "<circle cx=\"9\" cy=\"20\" r=\"1.2\"/><circle cx=\"18\" cy=\"20\" r=\"1.2\"/><path d=\"M1 2h3l2.4 12.4a2 2 0 0 0 2 1.6h8.9a2 2 0 0 0 2-1.6L21 7H5.2\"/>",
        ["food"] = "<path d=\"M18 8h1a4 4 0 0 1 0 8h-1\"/><path d=\"M2 8h16v8a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4Z\"/><line x1=\"6\" y1=\"2\" x2=\"6\" y2=\"5\"/><line x1=\"10\" y1=\"2\" x2=\"10\" y2=\"5\"/><line x1=\"14\" y1=\"2\" x2=\"14\" y2=\"5\"/>",
        ["coffee"] = "<path d=\"M4 8h13v6a5 5 0 0 1-5 5H9a5 5 0 0 1-5-5Z\"/><path d=\"M17 9h1.5a2.5 2.5 0 0 1 0 5H17\"/><path d=\"M7 2c0 1-1 1-1 2s1 1 1 2\"/><path d=\"M11 2c0 1-1 1-1 2s1 1 1 2\"/>",
        ["car"] = "<path d=\"M4 17H2v-4l2-5h11l4 5h1a2 2 0 0 1 2 2v2h-2\"/><circle cx=\"7\" cy=\"17\" r=\"2\"/><circle cx=\"17\" cy=\"17\" r=\"2\"/><path d=\"M9 17h6\"/>",
        ["plane"] = "<path d=\"M13 4 3 10l4 1 1 4 2-3 3 6 2-13Z\"/>",
        ["home"] = "<path d=\"M3 10.5 12 3l9 7.5\"/><path d=\"M5 9.5V20a1 1 0 0 0 1 1h4v-6h4v6h4a1 1 0 0 0 1-1V9.5\"/>",
        ["heart"] = "<path d=\"M20.5 5.5a5 5 0 0 0-8.5-2 5 5 0 0 0-8.5 5c0 6 8.5 11 8.5 11s8.5-5 8.5-11a5 5 0 0 0 0-3Z\"/>",
        ["health"] = "<path d=\"M20.5 6.5a5.5 5.5 0 0 0-9-2 5.5 5.5 0 0 0-9.5 5.5c0 6.5 9.5 12 9.5 12s9.5-5.5 9.5-12a5.5 5.5 0 0 0-.5-3.5Z\"/><path d=\"M9 11h6M12 8v6\"/>",
        ["gift"] = "<rect x=\"3\" y=\"8\" width=\"18\" height=\"13\" rx=\"1\"/><path d=\"M12 8v13\"/><path d=\"M19 8V6a2 2 0 0 0-2-2c-2 0-3 1.5-5 4-2-2.5-3-4-5-4a2 2 0 0 0-2 2v2\"/>",
        ["paw"] = "<circle cx=\"9\" cy=\"5\" r=\"1.8\"/><circle cx=\"15\" cy=\"5\" r=\"1.8\"/><circle cx=\"4.5\" cy=\"10\" r=\"1.8\"/><circle cx=\"19.5\" cy=\"10\" r=\"1.8\"/><path d=\"M12 12c-3 0-6 2-6 5.5S8.5 21 12 21s6-1.5 6-3.5S15 12 12 12Z\"/>",
        ["phone"] = "<rect x=\"7\" y=\"2\" width=\"10\" height=\"20\" rx=\"2\"/><line x1=\"11\" y1=\"18\" x2=\"13\" y2=\"18\"/>",
        ["book"] = "<path d=\"M4 4.5A2.5 2.5 0 0 1 6.5 2H20v17H6.5A2.5 2.5 0 0 0 4 21.5Z\"/><path d=\"M4 4.5v17\"/>",
        ["music"] = "<path d=\"M9 18V5l12-2v13\"/><circle cx=\"6\" cy=\"18\" r=\"3\"/><circle cx=\"18\" cy=\"16\" r=\"3\"/>",
        ["gamepad"] = "<rect x=\"2\" y=\"7\" width=\"20\" height=\"10\" rx=\"5\"/><line x1=\"6\" y1=\"12\" x2=\"10\" y2=\"12\"/><line x1=\"8\" y1=\"10\" x2=\"8\" y2=\"14\"/><circle cx=\"16\" cy=\"10.5\" r=\"1\"/><circle cx=\"18\" cy=\"12.5\" r=\"1\"/>",
        ["gym"] = "<path d=\"M6 7v10M18 7v10M2 10v4M22 10v4M6 12h12\"/>",
        ["baby"] = "<circle cx=\"12\" cy=\"7\" r=\"4.5\"/><path d=\"M5 21v-2c0-2.8 3.1-5 7-5s7 2.2 7 5v2\"/>",
        ["tools"] = "<path d=\"M14.5 6.5a3.5 3.5 0 0 0-4.7 4.7L3 18l3 3 6.8-6.8a3.5 3.5 0 0 0 4.7-4.7l-2.2 2.2-2-2Z\"/>",
        ["briefcase"] = "<rect x=\"2\" y=\"7\" width=\"20\" height=\"13\" rx=\"2\"/><path d=\"M16 7V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v2\"/>",
        ["tag"] = "<path d=\"M20.6 12.1 12 3.5H4v8l8.6 8.6a2 2 0 0 0 2.8 0l5.2-5.2a2 2 0 0 0 0-2.8Z\"/><circle cx=\"8\" cy=\"8\" r=\"1.2\"/>",
        ["wallet"] = "<path d=\"M3 7a2 2 0 0 1 2-2h13a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2Z\"/><path d=\"M16 12h3v3h-3a1.5 1.5 0 0 1 0-3Z\"/>",
        ["more"] = "<circle cx=\"5\" cy=\"12\" r=\"1.5\"/><circle cx=\"12\" cy=\"12\" r=\"1.5\"/><circle cx=\"19\" cy=\"12\" r=\"1.5\"/>",
    };
}
