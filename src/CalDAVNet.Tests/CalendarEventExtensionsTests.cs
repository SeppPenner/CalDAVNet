// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalendarEventExtensionsTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="CalendarEventExtensions" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Tests;

/// <summary>
/// A class to test the <see cref="CalendarEventExtensions"/> class.
/// </summary>
[TestClass]
public class CalendarEventExtensionsTests
{
    /// <summary>
    /// Checks whether the fields that describe the event end up in the JSON string. Up to version 1.0.3.0 the
    /// whole event was handed to the serializer, which threw an InvalidOperationException for every event.
    /// </summary>
    [TestMethod]
    public void ToJsonStringWritesTheFieldsOfTheEvent()
    {
        var calendarEvent = new CalendarEvent
        {
            Uid = "the-uid",
            Summary = "Test event",
            Description = "Test description",
            Location = "Test location"
        };

        var json = calendarEvent.ToJsonString();

        StringAssert.Contains(json, "\"Uid\":\"the-uid\"");
        StringAssert.Contains(json, "\"Summary\":\"Test event\"");
        StringAssert.Contains(json, "\"Description\":\"Test description\"");
        StringAssert.Contains(json, "\"Location\":\"Test location\"");
        StringAssert.Contains(json, "\"Attendees\":[]");
    }

    /// <summary>
    /// Checks whether a start date keeps its local time and the offset of its time zone.
    /// </summary>
    [TestMethod]
    public void ToJsonStringWritesTheStartWithItsTimeZoneOffset()
    {
        var calendarEvent = new CalendarEvent
        {
            Summary = "Test event",
            Start = new CalDateTime(2026, 8, 11, 10, 0, 0, "Europe/Berlin")
        };

        var json = calendarEvent.ToJsonString();

        StringAssert.Contains(json, "\"Start\":\"2026-08-11T10:00:00+02:00\"");
        StringAssert.Contains(json, "\"End\":null");
    }

    /// <summary>
    /// Checks whether the organizer and the attendees are written as their addresses.
    /// </summary>
    [TestMethod]
    public void ToJsonStringWritesTheOrganizerAndTheAttendees()
    {
        var calendarEvent = new CalendarEvent
        {
            Summary = "Test event",
            Organizer = new Organizer("mailto:organizer@localhost")
        };
        calendarEvent.Attendees.Add(new Attendee("mailto:attendee@localhost"));

        var json = calendarEvent.ToJsonString();

        StringAssert.Contains(json, "\"Organizer\":\"mailto:organizer@localhost\"");
        StringAssert.Contains(json, "\"Attendees\":[\"mailto:attendee@localhost\"]");
    }

    /// <summary>
    /// Checks whether an event without any data at all is serialized instead of throwing.
    /// </summary>
    [TestMethod]
    public void ToJsonStringWorksForAnEmptyEvent()
    {
        var json = new CalendarEvent().ToJsonString();

        StringAssert.Contains(json, "\"Summary\":null");
        StringAssert.Contains(json, "\"Start\":null");
    }
}
