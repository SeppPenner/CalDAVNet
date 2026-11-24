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
    /// Gets the events of the calendar at <see cref="Uri"/> that overlap the given time range. Only these events
    /// are transferred, a <see cref="Calendar"/> from the calendar methods always carries all of its events.
    /// </summary>
    /// <param name="start">The inclusive start of the time range.</param>
    /// <param name="end">The exclusive end of the time range.</param>
    /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="CalendarEvent"/>s.</returns>
    /// <exception cref="ArgumentException">Thrown if the end is not after the start.</exception>
    public Task<IEnumerable<CalendarEvent>> GetEvents(DateTimeOffset start, DateTimeOffset end)
    {
        return this.GetEventsInTimeRange(string.Empty, start, end);
    }

    /// <summary>
    /// Gets the events of the given calendar that overlap the given time range.
    /// </summary>
    /// <param name="calendar">The calendar.</param>
    /// <param name="start">The inclusive start of the time range.</param>
    /// <param name="end">The exclusive end of the time range.</param>
    /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="CalendarEvent"/>s.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the calendar is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if the end is not after the start.</exception>
    public Task<IEnumerable<CalendarEvent>> GetEvents(Calendar calendar, DateTimeOffset start, DateTimeOffset end)
    {
        ArgumentNullException.ThrowIfNull(calendar);
        return this.GetEventsInTimeRange(calendar.Uri, start, end);
    }

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
    /// Gets the events of a calendar that overlap the given time range.
    /// </summary>
    /// <param name="uri">The uri of the calendar.</param>
    /// <param name="start">The inclusive start of the time range.</param>
    /// <param name="end">The exclusive end of the time range.</param>
    /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="CalendarEvent"/>s.</returns>
    /// <exception cref="ArgumentException">Thrown if the end is not after the start.</exception>
    private Task<IEnumerable<CalendarEvent>> GetEventsInTimeRange(string uri, DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
        {
            throw new ArgumentException("The end of the time range has to be after its start.", nameof(end));
        }

        return this.QueryEvents(uri, start, end);
    }

    /// <summary>
    /// Gets the event url.
    /// </summary>
    /// <param name="calendarEvent">The calendar event.</param>
    /// <returns>The event url.</returns>
    private string GetEventUrl(CalendarEvent calendarEvent)
    {
        // The trailing slash is cut off because the uri may or may not carry one and a double
        // slash in the path is a different resource for most servers.
        var uri = this.Uri.ToString().TrimEnd('/');
        return $"{uri}/{calendarEvent.Uid}.ics";
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

        // Ical.Net returns a nullable string since version 5.0.0, an unserializable calendar
        // must not turn into a null body.
        return calendarSerializer.SerializeToString(calendar) ?? string.Empty;
    }
}
