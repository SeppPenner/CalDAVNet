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
    /// Gets the string representation of the <see cref="CalendarEvent"/>. Only the fields that describe the
    /// event are written. The event itself cannot be handed to the <see cref="JsonSerializer"/>, its object
    /// graph contains a <see cref="Encoding"/>, which the serializer refuses, and it points back to its own
    /// calendar.
    /// </summary>
    /// <param name="calendarEvent">The calendar event.</param>
    /// <returns>The JSON string of the <see cref="CalendarEvent"/>.</returns>
    public static string ToJsonString(this CalendarEvent calendarEvent)
    {
        return JsonSerializer.Serialize(new
        {
            calendarEvent.Uid,
            calendarEvent.Summary,
            calendarEvent.Description,
            calendarEvent.Location,
            Start = ToDateTimeOffset(calendarEvent.Start),
            End = ToDateTimeOffset(calendarEvent.End),
            calendarEvent.IsAllDay,
            calendarEvent.Status,
            Created = ToDateTimeOffset(calendarEvent.Created),
            LastModified = ToDateTimeOffset(calendarEvent.LastModified),
            Organizer = calendarEvent.Organizer?.Value?.ToString(),
            Attendees = calendarEvent.Attendees.Select(attendee => attendee.Value?.ToString()).ToList()
        });
    }

    /// <summary>
    /// Converts a calendar date and time into a <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="value">The calendar date and time.</param>
    /// <returns>The <see cref="DateTimeOffset"/> or <c>null</c> if none was given.</returns>
    private static DateTimeOffset? ToDateTimeOffset(CalDateTime? value)
    {
        if (value is null)
        {
            return null;
        }

        // The offset of the time zone is the distance between the local value and the same moment in UTC. It is
        // rounded to whole minutes because a DateTimeOffset does not accept anything else.
        var offset = TimeSpan.FromMinutes(Math.Round((value.Value - value.AsUtc).TotalMinutes));
        return new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified), offset);
    }
}
