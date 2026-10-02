namespace GastaApp.Helpers;

/// <summary>
/// Bundled payment-method logo assets under wwwroot/images/payments/.
///
/// Two separate sets, because a payment method can have two different logo shapes:
/// - Logos: wide rectangular wordmarks (brand name baked into the artwork), always
///   .svg, filename has no extension here — matches the original convention.
/// - IconLogos: square/circular mark-only variants, for contexts where a rectangle
///   doesn't fit (circular badges).
///
/// A method only needs an entry in IconLogos if a square asset actually exists for
/// it — anything missing here just falls back to the wordmark (see PaymentMethodIcon's
/// PreferIcon logic), so this list can be filled in gradually, one brand at a time.
/// </summary>
public static class PaymentLogoSet
{
    public static readonly string[] Logos =
    [
        "gcash", "creditcard", "cash", "maya", "debitcard", "maribank", "gotyme", "shopeepay"
    ];

    public static readonly Dictionary<string, string> IconLogos = new()
    {
        ["gcash"] = "gcash-icon",
        ["creditcard"] = "mastercard-icon",
        ["cash"] = "cash-icon",
        ["maya"] = "maya",
        ["debitcard"] = "mastercard-icon",
        ["maribank"] = "maribank-icon",
        ["gotyme"] = "gotyme-icon",
        ["shopeepay"] = "shopeepay-icon",
    };
}