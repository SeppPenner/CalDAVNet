// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Resource.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The resource model class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Model;

/// <summary>
/// The resource model class.
/// </summary>
internal class Resource
{
    /// <summary>
    /// Gets or sets the properties.
    /// </summary>
    public IReadOnlyDictionary<XName, string> Properties { get; set; } = new Dictionary<XName, string>();

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the uri.
    /// </summary>
    public string Uri { get; set; } = string.Empty;
}
