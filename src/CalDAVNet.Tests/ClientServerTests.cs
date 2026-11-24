// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientServerTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The tests of the client against fake CalDAV servers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Tests;

/// <summary>
/// The tests of the client against fake CalDAV servers. The SOGo server is modelled after
/// https://github.com/SeppPenner/CalDAVNet/issues/1: the calendars live in a calendar home below the principal,
/// the calendar names no owner, a report on anything but a calendar fails with 501 and a calendar query without a
/// component filter for the events answers with an empty multistatus.
/// </summary>
[TestClass]
public class ClientServerTests
{
    /// <summary>
    /// The address the SOGo client is created with, the calendar itself as in the issue.
    /// </summary>
    private const string SogoBaseUri = "https://sogo.example.com/SOGo/dav/user@example.com/Calendar/personal/";

    /// <summary>
    /// The principal of the SOGo user.
    /// </summary>
    private const string SogoPrincipal = "/SOGo/dav/user@example.com/";

    /// <summary>
    /// The calendar home of the SOGo user.
    /// </summary>
    private const string SogoCalendarHome = "/SOGo/dav/user@example.com/Calendar/";

    /// <summary>
    /// The calendar of the SOGo user.
    /// </summary>
    private const string SogoCalendar = "/SOGo/dav/user@example.com/Calendar/personal/";

    /// <summary>
    /// The principal of the server without a calendar home set.
    /// </summary>
    private const string PlainPrincipal = "/caldav.php/user/";

    /// <summary>
    /// The calendar of the server without a calendar home set.
    /// </summary>
    private const string PlainCalendar = "/caldav.php/user/calendar/";

    /// <summary>
    /// The calendar data of the one event, with the encoded carriage returns SOGo writes and an escaped ampersand.
    /// </summary>
    private const string EventData = """
        BEGIN:VCALENDAR&#13;
        VERSION:2.0&#13;
        PRODID:-//Inverse inc.//SOGo 5.0//EN&#13;
        BEGIN:VEVENT&#13;
        UID:event-1&#13;
        DTSTAMP:20260301T080000Z&#13;
        DTSTART:20260310T090000Z&#13;
        DTEND:20260310T100000Z&#13;
        SUMMARY:Tom &amp; Jerry&#13;
        END:VEVENT&#13;
        END:VCALENDAR&#13;
        """;

    /// <summary>
    /// Checks whether all calendars are taken out of the calendar home and not out of the principal, and whether
    /// the event of the calendar arrives with its decoded summary.
    /// </summary>
    [TestMethod]
    public async Task GetAllCalendarsFindsTheCalendarInTheCalendarHome()
    {
        var client = CreateClient(SogoBaseUri, new FakeHttpMessageHandler(AnswerLikeSogo));

        var calendar = (await client.GetAllCalendars()).Single();

        Assert.AreEqual(SogoCalendar, calendar.Uri);
        Assert.AreEqual("Personal Calendar", calendar.DisplayName);
        Assert.AreEqual(string.Empty, calendar.Owner);
        Assert.AreEqual("Tom & Jerry", calendar.Events.Single().Summary);
    }

    /// <summary>
    /// Checks whether the default calendar is the first calendar of the calendar home and not the principal.
    /// </summary>
    [TestMethod]
    public async Task GetDefaultCalendarReturnsTheFirstCalendarAndNotThePrincipal()
    {
        var client = CreateClient(SogoBaseUri, new FakeHttpMessageHandler(AnswerLikeSogo));

        var calendar = await client.GetDefaultCalendar();

        Assert.IsNotNull(calendar);
        Assert.AreEqual(SogoCalendar, calendar.Uri);
        Assert.HasCount(1, calendar.Events);
    }

    /// <summary>
    /// Checks whether a server without a calendar home set still has its calendars found below the principal,
    /// while the principal itself is no calendar.
    /// </summary>
    [TestMethod]
    public async Task GetAllCalendarsFallsBackToThePrincipalWithoutACalendarHomeSet()
    {
        var client = CreateClient("http://192.168.2.2/caldav.php/user/", new FakeHttpMessageHandler(AnswerWithoutCalendarHomeSet));

        var calendar = (await client.GetAllCalendars()).Single();

        Assert.AreEqual(PlainCalendar, calendar.Uri);
        Assert.AreEqual("Plain calendar", calendar.DisplayName);
    }

    /// <summary>
    /// Checks whether the events of a time range are asked for at the address of the client with a time range in
    /// UTC inside the component filter for the events.
    /// </summary>
    [TestMethod]
    public async Task GetEventsAsksForTheEventsOfTheTimeRange()
    {
        var handler = new FakeHttpMessageHandler(AnswerLikeSogo);
        var client = CreateClient(SogoBaseUri, handler);
        var start = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.FromHours(1));
        var end = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(2));

        var events = await client.GetEvents(start, end);

        Assert.AreEqual("Tom & Jerry", events.Single().Summary);
        Assert.AreEqual("REPORT", handler.SentRequest?.Method.Method);
        Assert.AreEqual(SogoBaseUri, handler.SentRequest?.RequestUri?.ToString());

        var timeRange = XDocument.Parse(handler.SentBodies.Last()).Descendants(Namespaces.CalNs + ElementNames.TimeRange).Single();
        Assert.AreEqual(ElementNames.VEvent, timeRange.Parent?.Attribute(ElementNames.Name)?.Value);
        Assert.AreEqual("20260228T230000Z", timeRange.Attribute(ElementNames.Start)?.Value);
        Assert.AreEqual("20260331T220000Z", timeRange.Attribute(ElementNames.End)?.Value);
    }

    /// <summary>
    /// Checks whether the events of a given calendar are asked for at the uri of that calendar.
    /// </summary>
    [TestMethod]
    public async Task GetEventsOfACalendarAsksAtTheUriOfTheCalendar()
    {
        var handler = new FakeHttpMessageHandler(AnswerLikeSogo);
        var client = CreateClient("https://sogo.example.com/", handler);
        var start = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);

        var events = await client.GetEvents(new Model.Public.Calendar { Uri = SogoCalendar }, start, start.AddDays(31));

        Assert.HasCount(1, events);
        Assert.AreEqual(SogoCalendar, handler.SentRequest?.RequestUri?.AbsolutePath);
    }

    /// <summary>
    /// Checks whether a time range whose end is not after its start and a missing calendar are rejected before a
    /// request goes out.
    /// </summary>
    [TestMethod]
    public void GetEventsRejectsAnEmptyTimeRangeAndAMissingCalendar()
    {
        var handler = new FakeHttpMessageHandler(AnswerLikeSogo);
        var client = CreateClient(SogoBaseUri, handler);
        var start = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);

        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            _ = client.GetEvents(start, start);
        });
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            _ = client.GetEvents(null!, start, start.AddDays(1));
        });
        Assert.IsEmpty(handler.SentBodies);
    }

    /// <summary>
    /// Creates a client that sends its requests to the given handler.
    /// </summary>
    /// <param name="uri">The uri of the client.</param>
    /// <param name="handler">The handler.</param>
    /// <returns>The <see cref="Client"/>.</returns>
    private static Client CreateClient(string uri, FakeHttpMessageHandler handler)
    {
        return new Client(new Uri(uri), TestDataProvider.UserName, TestDataProvider.Password, new HttpClient(handler));
    }

    /// <summary>
    /// Answers a request the way the SOGo server of the issue does.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="body">The body of the request.</param>
    /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
    private static HttpResponseMessage AnswerLikeSogo(HttpRequestMessage request, string body)
    {
        var path = request.RequestUri?.AbsolutePath;
        var depth = request.Headers.GetValues(HeaderNames.Depth).Single();
        var propfind = request.Method.Method == HttpMethods.PropFind;

        if (propfind && body.Contains(ElementNames.CurrentUserPrincipal))
        {
            return MultiStatus(Response(path, $"<D:current-user-principal><D:href>{SogoPrincipal}</D:href></D:current-user-principal>"));
        }

        if (propfind && path == SogoPrincipal && body.Contains(ElementNames.CalendarHomeSet))
        {
            return MultiStatus(Response(SogoPrincipal, $"<C:calendar-home-set><D:href>{SogoCalendarHome}</D:href></C:calendar-home-set>"));
        }

        if (propfind && path == SogoCalendarHome && depth == HeaderValues.One)
        {
            return MultiStatus(
                Response(SogoCalendarHome, "<D:resourcetype><D:collection /></D:resourcetype><D:displayname>Calendar</D:displayname>") +
                Response(SogoCalendar, "<D:resourcetype><D:collection /><C:calendar /></D:resourcetype><D:displayname>Personal Calendar</D:displayname>"));
        }

        if (propfind && path == SogoCalendar && depth == HeaderValues.Zero)
        {
            return MultiStatus(Response(SogoCalendar, "<D:resourcetype><D:collection /><C:calendar /></D:resourcetype><D:displayname>Personal Calendar</D:displayname>"));
        }

        if (request.Method.Method == HttpMethods.Report && path == SogoCalendar)
        {
            return body.Contains(ElementNames.VEvent)
                ? MultiStatus(Response($"{SogoCalendar}event-1.ics", $"<D:getetag>\"gcs00000001\"</D:getetag><C:calendar-data>{EventData}</C:calendar-data>"))
                : MultiStatus(string.Empty);
        }

        return TestDataProvider.GetResponseMessage(HttpStatusCode.NotImplemented, string.Empty);
    }

    /// <summary>
    /// Answers a request the way a server without a calendar home set does, its calendars live below the principal.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="body">The body of the request.</param>
    /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
    private static HttpResponseMessage AnswerWithoutCalendarHomeSet(HttpRequestMessage request, string body)
    {
        var path = request.RequestUri?.AbsolutePath;
        var depth = request.Headers.GetValues(HeaderNames.Depth).Single();
        var propfind = request.Method.Method == HttpMethods.PropFind;

        if (propfind && body.Contains(ElementNames.CurrentUserPrincipal))
        {
            return MultiStatus(Response(path, $"<D:current-user-principal><D:href>{PlainPrincipal}</D:href></D:current-user-principal>"));
        }

        if (propfind && body.Contains(ElementNames.CalendarHomeSet))
        {
            return MultiStatus($"<D:response><D:href>{PlainPrincipal}</D:href><D:propstat><D:prop><C:calendar-home-set /></D:prop><D:status>HTTP/1.1 404 Not Found</D:status></D:propstat></D:response>");
        }

        if (propfind && path == PlainPrincipal && depth == HeaderValues.One)
        {
            return MultiStatus(
                Response(PlainPrincipal, "<D:resourcetype><D:collection /><D:principal /></D:resourcetype>") +
                Response(PlainCalendar, "<D:resourcetype><D:collection /><C:calendar /></D:resourcetype>"));
        }

        if (propfind && path == PlainCalendar && depth == HeaderValues.Zero)
        {
            return MultiStatus(Response(PlainCalendar, "<D:displayname>Plain calendar</D:displayname><D:getcontenttype>httpd/unix-directory</D:getcontenttype>"));
        }

        if (request.Method.Method == HttpMethods.Report && path == PlainCalendar)
        {
            return MultiStatus(string.Empty);
        }

        return TestDataProvider.GetResponseMessage(HttpStatusCode.NotImplemented, string.Empty);
    }

    /// <summary>
    /// Wraps responses into a multistatus answer.
    /// </summary>
    /// <param name="responses">The response elements.</param>
    /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
    private static HttpResponseMessage MultiStatus(string responses)
    {
        return TestDataProvider.GetResponseMessage(
            HttpStatusCode.MultiStatus,
            $"""<?xml version="1.0" encoding="utf-8"?><D:multistatus xmlns:D="DAV:" xmlns:C="urn:ietf:params:xml:ns:caldav">{responses}</D:multistatus>""");
    }

    /// <summary>
    /// Builds a response element with one successful propstat block, the status first as SOGo writes it.
    /// </summary>
    /// <param name="href">The href of the resource.</param>
    /// <param name="properties">The properties.</param>
    /// <returns>The response element as <see cref="string"/>.</returns>
    private static string Response(string? href, string properties)
    {
        return $"<D:response><D:href>{href}</D:href><D:propstat><D:status>HTTP/1.1 200 OK</D:status><D:prop>{properties}</D:prop></D:propstat></D:response>";
    }
}
