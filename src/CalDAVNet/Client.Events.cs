// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Client.Events.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The client class for event data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet;

/// <summary>
/// The client class for event data.
/// </summary>
public partial class Client
{
    /// <summary>
    /// The calendar serializer.
    /// </summary>
    private static readonly CalendarSerializer calendarSerializer = new();

    /// <summary>
    /// Deletes an event.
    /// </summary>
    /// <param name="calendarEvent">The calendar event.</param>
    /// <returns>A value indicating whether the event was deleted or not.</returns>
    public async Task<bool> DeleteEvent(CalendarEvent calendarEvent)
    {
        var result = await this.client
            .Delete(this.GetEventUrl(calendarEvent))
            .Send()
            .ConfigureAwait(false);

        return result.IsSuccessful;
    }

    /// <summary>
    /// Deletes an event.
    /// </summary>
    /// <param name="calendarEvent">The calendar event.</param>
    /// <param name="calendar">The calendar.</param>
    /// <returns>A value indicating whether the event was added or updated or not.</returns>
    public async Task<bool> AddOrUpdateEvent(CalendarEvent calendarEvent, Ical.Net.Calendar calendar)
    {
        var result = await this.client
            .Put(this.GetEventUrl(calendarEvent), this.Serialize(calendarEvent, calendar))
            .Send()
            .ConfigureAwait(false);

        return result.IsSuccessful;
    }

    /// <summary>
    /// Gets the event url.
    /// </summary>
    /// <param name="calendarEvent">The calendar event.</param>
    /// <returns>The event url.</returns>
    private string GetEventUrl(CalendarEvent calendarEvent)
    {
        return $"{this.Uri}/{calendarEvent.Uid}.ics";
    }

    /// <summary>
    /// Serializes the event.
    /// </summary>
    /// <param name="calendarEvent">The calendar event.</param>
    /// <param name="calendar">The calendar.</param>
    /// <returns>The serialized event as <see cref="string"/>.</returns>
    private string Serialize(CalendarEvent calendarEvent, Ical.Net.Calendar calendar)
    {
        calendar.Events.Clear();
        calendar.Events.Add(calendarEvent);
        return calendarSerializer.SerializeToString(calendar);
    }
}
