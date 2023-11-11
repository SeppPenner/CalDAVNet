// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Calendar.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The calendar model class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Model.Public;

/// <summary>
/// The calendar model class.
/// </summary>
public sealed record class Calendar
{
    /// <summary>
    /// Gets or sets the uid.
    /// </summary>
    public string Uid { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date of the last modification.
    /// </summary>
    public DateTimeOffset LastModified { get; set; }

    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the uri.
    /// </summary>
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the E tag.
    /// </summary>
    public string ETag { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sync token.
    /// </summary>
    public string SyncToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTimeOffset CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the events.
    /// </summary>
    public List<CalendarEvent> Events { get; set; } = new();
}
