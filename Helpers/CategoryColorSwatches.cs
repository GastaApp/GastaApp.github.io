namespace GastaApp.Helpers;

/// <summary>
/// Curated category color swatches — every value is already in the ~55-75% lightness
/// band with decent saturation, the same range we deepened the seed categories into
/// after the original palette was too pale (blended into surfaces, poor contrast for
/// white text on headers/buttons). A free-form color picker would let users recreate
/// that exact bug, so this is a fixed, pre-vetted set instead.
/// </summary>
public static class CategoryColorSwatches
{
    public static readonly string[] Colors =
    {
        "#FF7A52", // coral-orange
        "#FF4FA3", // magenta-pink
        "#4ECD82", // green
        "#FFA94D", // amber
        "#E8B93E", // gold
        "#FF8FAE", // rose
        "#45C7D1", // cyan
        "#A968EE", // purple
        "#5B8DEF", // blue
        "#EF5B5B", // red
        "#5BD1C0", // teal
        "#F2A65A", // tan-orange
        "#8E6C88", // muted plum
        "#63C7B2", // mint
    };
}
