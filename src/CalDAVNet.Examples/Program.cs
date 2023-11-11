//--------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The main program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Examples;

/// <summary>
/// The main program.
/// </summary>
public class Program
{
    /// <summary>
    /// The main method.
    /// </summary>
    public static async Task Main()
    {
        // Create client.
        var calDavClient = new Client("http://192.168.2.205", "test", "password");

        // Get all calendars for the user.
        var calendars = await calDavClient.GetAllCalendars();

        // Get the calendar by the uid.
        var calendarByUid = await calDavClient.GetCalendarByUid("test");

        // Get the default calendar.
        var defaultCalendar = await calDavClient.GetDefaultCalendar();

        // Add an event.
        var calendarEvent = new CalendarEvent();
        var added = await calDavClient.AddOrUpdateEvent(calendarEvent, new Ical.Net.Calendar());

        // Delete an event.
        var deleted = await calDavClient.DeleteEvent(calendarEvent);
    }
}
