// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Client.Calendar.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The client class for calendar data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet;

/// <summary>
/// The client class for calendar data.
/// </summary>
public partial class Client
{
    /// <summary>
    /// The href code regex.
    /// </summary>
    [GeneratedRegex("<[^>]*(>|$)")]
    private static partial Regex HrefRegex();

    /// <summary>
    /// The href code regex.
    /// </summary>
    private static readonly Regex hrefRegex = HrefRegex();

    /// <summary>
    /// The date time format.
    /// </summary>
    private const string DateTimeFormat = "yyyyMMddTHHmmssZ";

    /// <summary>
    /// The format of the start and the end of a time range, a UTC date time as RFC 4791 demands it.
    /// </summary>
    private const string TimeRangeFormat = "yyyyMMdd'T'HHmmss'Z'";

    /// <summary>
    /// Gets all calendars available for the current user (or none if unauthenticated).
    /// </summary>
    /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="Calendar"/>s.</returns>
    public async Task<IEnumerable<Calendar>> GetAllCalendars()
    {
        // Get all calendars by uri.
        var calendars = new List<Calendar>();

        foreach (var uri in await this.GetCalendarUris())
        {
            var calendar = await this.GetCalendarWithUri(uri);

            if (calendar is null)
            {
                continue;
            }

            calendars.Add(calendar);
        }

        return calendars;
    }

    /// <summary>
    /// Gets a calendar by its uid.
    /// </summary>
    /// <param name="uid">The uid of the calendar.</param>
    /// <returns>The <see cref="Calendar"/> or <c>null</c> if none was found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the uid is empty.</exception>
    public Task<Calendar?> GetCalendarByUid(string uid)
    {
        if (string.IsNullOrWhiteSpace(uid))
        {
            throw new ArgumentNullException(nameof(uid));
        }

        return this.GetCalendarWithUri(uid);
    }

    /// <summary>
    /// Gets the default calendar for the given user, the first calendar in the calendar home.
    /// </summary>
    /// <returns>The <see cref="Calendar"/> or <c>null</c> if none was found.</returns>
    public async Task<Calendar?> GetDefaultCalendar()
    {
        // The first calendar in the calendar home is the default one. The principal itself is no calendar,
        // a report on it fails, see https://github.com/SeppPenner/CalDAVNet/issues/1.
        foreach (var uri in await this.GetCalendarUris())
        {
            var calendar = await this.GetCalendarWithUri(uri);

            if (calendar is not null)
            {
                return calendar;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether the resource names itself a calendar collection in its resource type (RFC 4791, section 4.2).
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <returns>A value indicating whether the resource is a calendar collection or not.</returns>
    private static bool IsCalendarCollection(Resource resource)
    {
        var resourceType = resource.Properties.FirstOrDefault(x => x.Key.LocalName == ElementNames.ResourceType).Value;
        return ParseFragment(resourceType).Any(x => x.Name == Namespaces.CalNs + ElementNames.Calendar);
    }

    /// <summary>
    /// Gets the first href out of the inner XML of a property, for example out of a calendar home set.
    /// </summary>
    /// <param name="innerXml">The inner XML of the property.</param>
    /// <returns>The href as <see cref="string"/> or <c>null</c> if there is none.</returns>
    private static string? GetFirstHref(string? innerXml)
    {
        var href = ParseFragment(innerXml).FirstOrDefault(x => x.Name.LocalName == ElementNames.Href)?.Value.Trim();
        return string.IsNullOrWhiteSpace(href) ? null : href;
    }

    /// <summary>
    /// Parses the inner XML of a property. The inner XML carries the namespace declarations of its elements,
    /// so it parses on its own once it is wrapped into a root element.
    /// </summary>
    /// <param name="innerXml">The inner XML of the property.</param>
    /// <returns>All elements of the inner XML as <see cref="IEnumerable{T}"/> of <see cref="XElement"/>.</returns>
    private static IEnumerable<XElement> ParseFragment(string? innerXml)
    {
        if (string.IsNullOrWhiteSpace(innerXml))
        {
            return [];
        }

        try
        {
            return XElement.Parse($"<{ElementNames.Prop}>{innerXml}</{ElementNames.Prop}>").Descendants().ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Gets the uris of the calendars of the current user. The calendars live in the calendar home that the
    /// principal names in its calendar home set (RFC 4791, section 6.2.1), a server that names none keeps them
    /// below the principal itself. A resource that names its resource type is only taken if it is a calendar
    /// collection, which drops the calendar home itself and collections such as address books.
    /// </summary>
    /// <returns>The uris of the calendars as <see cref="IReadOnlyList{T}"/> of <see cref="string"/>.</returns>
    private async Task<IReadOnlyList<string>> GetCalendarUris()
    {
        // Get the user uri.
        var userUri = await this.GetUserUri();

        if (string.IsNullOrWhiteSpace(userUri))
        {
            return [];
        }

        var homeUri = await this.GetCalendarHomeUri(userUri) ?? userUri;

        // Create the request body.
        var prop = new XElement(Namespaces.DavNs + ElementNames.Prop);
        prop.Add(new XElement(Namespaces.DavNs + ElementNames.ResourceType));
        prop.Add(new XElement(Namespaces.DavNs + ElementNames.DisplayName));
        prop.Add(new XElement(Namespaces.ServerNs + ElementNames.GetCTag));
        prop.Add(new XElement(Namespaces.CalNs + ElementNames.SupportedCalendarComponentSet));

        var root = new XElement(
            Namespaces.DavNs + ElementNames.PropFind,
            new XAttribute(XNamespace.Xmlns + ElementNames.D, Namespaces.DavNs),
            new XAttribute(XNamespace.Xmlns + ElementNames.C, Namespaces.CalNs),
            new XAttribute(XNamespace.Xmlns + ElementNames.Cs, Namespaces.ServerNs));
        root.Add(prop);

        // Query for data.
        var result = await this.client
            .Propfind(homeUri, root)
            .Send()
            .ConfigureAwait(false);

        if (!result.IsSuccessful)
        {
            return [];
        }

        return result.Resources
            .Where(x => !x.Properties.Any(p => p.Key.LocalName == ElementNames.ResourceType) || IsCalendarCollection(x))
            .Select(x => x.Uri)
            .ToList();
    }

    /// <summary>
    /// Gets the uri of the calendar home out of the calendar home set of the principal.
    /// </summary>
    /// <param name="userUri">The uri of the principal.</param>
    /// <returns>The uri of the calendar home as <see cref="string"/> or <c>null</c> if the server names none.</returns>
    private async Task<string?> GetCalendarHomeUri(string userUri)
    {
        // Create the request body.
        var prop = new XElement(Namespaces.DavNs + ElementNames.Prop);
        prop.Add(new XElement(Namespaces.CalNs + ElementNames.CalendarHomeSet));

        var root = new XElement(
            Namespaces.DavNs + ElementNames.PropFind,
            new XAttribute(XNamespace.Xmlns + ElementNames.D, Namespaces.DavNs),
            new XAttribute(XNamespace.Xmlns + ElementNames.C, Namespaces.CalNs));
        root.Add(prop);

        // Query for data.
        var result = await this.client
            .Propfind(userUri, root)
            .WithHeader(HeaderNames.Depth, HeaderValues.Zero)
            .Send()
            .ConfigureAwait(false);

        if (!result.IsSuccessful)
        {
            return null;
        }

        var homeSet = result.Resources.FirstOrDefault()?.Properties.FirstOrDefault(x => x.Key.LocalName == ElementNames.CalendarHomeSet).Value;
        return GetFirstHref(homeSet);
    }

    /// <summary>
    /// Gets a calendar by its uri.
    /// </summary>
    /// <param name="uri">The uri of the calendar.</param>
    /// <returns>The <see cref="Calendar"/> or <c>null</c> if none was found.</returns>
    private async Task<Calendar?> GetCalendarWithUri(string uri)
    {
        // Create the request body.
        var propfind = new XElement(Namespaces.DavNs + ElementNames.PropFind, new XAttribute(XNamespace.Xmlns + ElementNames.D, Namespaces.DavNs));
        propfind.Add(new XElement(Namespaces.DavNs + ElementNames.AllProps));

        // Query for data. Only the calendar itself is asked for, a depth of one would list every event in it.
        var result = await this.client
            .Propfind(uri, propfind)
            .WithHeader(HeaderNames.Depth, HeaderValues.Zero)
            .Send()
            .ConfigureAwait(false);

        if (!result.IsSuccessful)
        {
            return null;
        }

        // Get resource.
        var resource = result.Resources.FirstOrDefault();

        if (resource is null)
        {
            return null;
        }

        // Check if the resource really is a calendar. A calendar collection says so in its resource type, the
        // content types are what the servers this library was first written against answer instead.
        var contentType = resource.Properties.FirstOrDefault(x => x.Key.LocalName == ElementNames.GetContentType).Value;

        if (!IsCalendarCollection(resource) && (contentType is null || !contentTypes.Contains(contentType)))
        {
            return null;
        }

        return await this.DeserializeCalendarResource(resource, uri, this.client);
    }

    /// <summary>
    /// Gets the user uri.
    /// </summary>
    /// <returns>The user uri as <see cref="string"/> or <c>null</c> if none was found.</returns>
    private async Task<string?> GetUserUri()
    {
        // Create the request body.
        var prop = new XElement(Namespaces.DavNs + ElementNames.Prop);
        prop.Add(new XElement(Namespaces.DavNs + ElementNames.CurrentUserPrincipal));

        var root = new XElement(Namespaces.DavNs + ElementNames.PropFind, new XAttribute(XNamespace.Xmlns + ElementNames.D, Namespaces.DavNs));
        root.Add(prop);

        // Query for data.
        var result = await this.client
            .Propfind(string.Empty, root)
            .WithHeader(HeaderNames.Depth, HeaderValues.Zero)
            .Send()
            .ConfigureAwait(false);

        if (!result.IsSuccessful)
        {
            return null;
        }

        // Get resource.
        var resource = result.Resources.FirstOrDefault();

        if (resource is null)
        {
            return null;
        }

        // Check the resource for the user uri property.
        foreach (var keyValue in resource.Properties)
        {
            if (keyValue.Key.LocalName == ElementNames.CurrentUserPrincipal)
            {
                return this.tagRegex.Replace(keyValue.Value, string.Empty).Trim();
            }
        }

        return null;
    }

    /// <summary>
    /// Deserializes the calendar resource.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="uri">The uri.</param>
    /// <param name="client">The CalDAV client.</param>
    /// <returns>The <see cref="Calendar"/> or <c>null</c> if none was found.</returns>
    private async Task<Calendar?> DeserializeCalendarResource(Resource resource, string uri, CalDAVClient client)
    {
        var calendar = new Calendar
        {
            Uri = uri
        };

        foreach (var property in resource.Properties)
        {
            switch (property.Key.LocalName)
            {
                case ElementNames.DisplayName:
                    calendar.DisplayName = property.Value;
                    break;

                case ElementNames.Owner:
                    calendar.Owner = property.Value.Contains(ElementNames.Href) ? hrefRegex.Replace(property.Value, string.Empty) : property.Value;
                    break;

                case ElementNames.GetETag:
                    calendar.ETag = property.Value;
                    break;

                case ElementNames.GetLastModified:
                    calendar.LastModified = DateTimeOffset.Parse(property.Value, CultureInfo.InvariantCulture);
                    break;

                case ElementNames.SyncToken:
                    calendar.SyncToken = property.Value;

                    break;

                case ElementNames.CalendarColor:
                    calendar.Color = property.Value;
                    break;

                case ElementNames.CalendarDescription:
                    calendar.Description = property.Value;
                    break;

                case ElementNames.CreationDate:
                    calendar.CreationDate = DateTimeOffset.ParseExact(property.Value, DateTimeFormat, CultureInfo.InvariantCulture);
                    break;
            }
        }

        // The owner is only cut out if the server sent one, string.Replace throws on an empty value.
        var uid = string.IsNullOrEmpty(calendar.Owner) ? uri : uri.Replace(calendar.Owner, string.Empty);
        calendar.Uid = uid.Replace("/", string.Empty);

        // Fetch the events.
        var events = await this.QueryEvents(uri, null, null).ConfigureAwait(false);
        calendar.Events = events.ToList();
        return calendar;
    }

    /// <summary>
    /// Gets the events of a calendar, all of them or only those that overlap a time range.
    /// </summary>
    /// <param name="uri">The uri of the calendar.</param>
    /// <param name="start">The inclusive start of the time range or <c>null</c> for all events.</param>
    /// <param name="end">The exclusive end of the time range or <c>null</c> for all events.</param>
    /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="CalendarEvent"/>s.</returns>
    private async Task<IEnumerable<CalendarEvent>> QueryEvents(string uri, DateTimeOffset? start, DateTimeOffset? end)
    {
        // Create the request body.
        var query = new XElement(Namespaces.CalNs + ElementNames.CalendarQuery, new XAttribute(XNamespace.Xmlns + ElementNames.D, Namespaces.DavNs), new XAttribute(XNamespace.Xmlns + ElementNames.C, Namespaces.CalNs));

        var prop = new XElement(Namespaces.DavNs + ElementNames.Prop);
        prop.Add(new XElement(Namespaces.DavNs + ElementNames.GetETag));
        prop.Add(new XElement(Namespaces.CalNs + ElementNames.CalendarData));
        query.Add(prop);

        // The events are asked for with a component filter of their own. A filter on the calendar alone is valid
        // (RFC 4791, section 7.8), but not every server answers it with events, see
        // https://github.com/SeppPenner/CalDAVNet/issues/1. Only events are handed out anyway.
        var eventFilter = new XElement(Namespaces.CalNs + ElementNames.CompFilter, new XAttribute(ElementNames.Name, ElementNames.VEvent));

        if (start.HasValue && end.HasValue)
        {
            eventFilter.Add(new XElement(
                Namespaces.CalNs + ElementNames.TimeRange,
                new XAttribute(ElementNames.Start, start.Value.UtcDateTime.ToString(TimeRangeFormat, CultureInfo.InvariantCulture)),
                new XAttribute(ElementNames.End, end.Value.UtcDateTime.ToString(TimeRangeFormat, CultureInfo.InvariantCulture))));
        }

        var filter = new XElement(Namespaces.CalNs + ElementNames.Filter);
        filter.Add(new XElement(Namespaces.CalNs + ElementNames.CompFilter, new XAttribute(ElementNames.Name, ElementNames.VCalendar), eventFilter));
        query.Add(filter);

        // Query for data.
        var result = await this.client
            .Report(uri, query)
            .Send()
            .ConfigureAwait(false);

        // Parse the events.
        return result.Resources
            .SelectMany(x => x.Properties)
            .Where(x => x.Key.LocalName == ElementNames.CalendarData)
            .SelectMany(x => Ical.Net.Calendar.Load<Ical.Net.Calendar>(x.Value))
            .SelectMany(x => x.Events);
    }
}
