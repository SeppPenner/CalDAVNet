// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XDocumentExtensions.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An extension class for the <see cref="XDocument"/> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Extensions;

/// <summary>
/// An extension class for the <see cref="XDocument"/> class.
/// </summary>
internal static class XDocumentExtensions
{
    /// <summary>
    /// Gets the <see cref="StringContent"/> of the <see cref="XDocument"/>.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <returns>The <see cref="StringContent"/>.</returns>
    public static StringContent ToStringContent(this XDocument document)
    {
        return new StringContent(document.ToString(), Encoding.UTF8, MediaTypeNames.Application.Xml);
    }
}
