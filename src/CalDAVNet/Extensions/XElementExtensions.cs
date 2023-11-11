// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XElementExtensions.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An extension class for the <see cref="XElement"/> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Extensions;

/// <summary>
/// An extension class for the <see cref="XElement"/> class.
/// </summary>
internal static partial class XElementExtensions
{
    /// <summary>
    /// The status code regex.
    /// </summary>
    [GeneratedRegex(".*(\\d{3}).*")]
    private static partial Regex StatusCodeRegex();

    /// <summary>
    /// The status code regex.
    /// </summary>
    private static readonly Regex statusCodeRegex = StatusCodeRegex();

    /// <summary>
    /// Gets the local name element.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="localName">The local name.</param>
    /// <param name="comparison">The comparison.</param>
    /// <returns>The <see cref="XElement"/>.</returns>
    public static XElement? LocalNameElement(this XElement element, string localName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        return element.Elements().FirstOrDefault(x => x.Name.LocalName.Equals(localName, comparison));
    }

    /// <summary>
    /// Gets the local name elements.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="localName">The local name.</param>
    /// <param name="comparison">The comparison.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="XElement"/>.</returns>
    public static IEnumerable<XElement> LocalNameElements(this XElement element, string localName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        return element.Elements().Where(x => x.Name.LocalName.Equals(localName, comparison));
    }

    /// <summary>
    /// Gets the inner XML.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>The inner XML <see cref="string"/>.</returns>
    public static string GetInnerXml(this XElement element)
    {
        using var reader = element.CreateReader();
        reader.MoveToContent();
        return reader.ReadInnerXml();
    }

    /// <summary>
    /// Gets the status code.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>The status code as <see cref="int"/>.</returns>
    public static int GetStatusCode(this XElement element)
    {
        var rawValue = element.LocalNameElement(ElementNames.Status)?.Value;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return -1;
        }

        var codeGroup = statusCodeRegex.Match(rawValue).Groups[1];

        if (!codeGroup.Success)
        {
            return -1;
        }

        if (!int.TryParse(codeGroup.Value, out var statusCode))
        {
            return -1;
        }

        return statusCode;
    }

    /// <summary>
    /// Gets the description.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>The description as <see cref="string"/>.</returns>
    public static string GetDescription(this XElement element)
    {
        return element.LocalNameElement(ElementNames.ResponseDescription)?.Value ?? element.LocalNameElement(ElementNames.Status)?.Value ?? string.Empty;
    }
}
