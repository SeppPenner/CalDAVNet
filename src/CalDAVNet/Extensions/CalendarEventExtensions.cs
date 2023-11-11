// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalendarEventExtensions.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An extension class for the <see cref="CalendarEvent"/> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Extensions;

/// <summary>
/// An extension class for the <see cref="CalendarEvent"/> class.
/// </summary>
public static class CalendarEventExtensions
{
    /// <summary>
    /// Gets the string representation of the <see cref="CalendarEvent"/>.
    /// </summary>
    /// <param name="calendarEvent">The calendar event.</param>
    /// <returns>The JSON string of the <see cref="CalendarEvent"/>.</returns>
    public static string ToJsonString(this CalendarEvent calendarEvent)
    {
        return JsonSerializer.Serialize(calendarEvent);
    }
}
