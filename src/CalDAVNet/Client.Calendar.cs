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
    /// Gets all calendars available for the current user (or none if unauthenticated).
    /// </summary>
    /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="Calendar"/>s.</returns>
    public async Task<IEnumerable<Calendar>> GetCalendars()
    {
        // Get the user uri.
        var userUri = await this.GetUserUri();

        if (string.IsNullOrWhiteSpace(userUri))
        {
            return new List<Calendar>();
        }

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
            .Propfind(userUri, root)
            .Send()
            .ConfigureAwait(false);

        if (!result.IsSuccessful)
        {
            return new List<Calendar>();
        }

        // Get all calendars by uri.
        var calendars = new List<Calendar>();

        foreach (var resource in result.Resources)
        {
            var calendar = await this.GetCalendarWithUri(resource.Uri);

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

        return this.GetCalendarWithUri($"{this.UserName}/{uid}");
    }

    /// <summary>
    /// Gets the default calendar for the given user.
    /// </summary>
    /// <returns>The <see cref="Calendar"/> or <c>null</c> if none was found.</returns>
    public async Task<Calendar?> GetDefaultCalendar()
    {
        // Get the user uri.
        var userUri = await this.GetUserUri();

        if (string.IsNullOrWhiteSpace(userUri))
        {
            return null;
        }

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
            .Propfind(userUri, root)
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

        // Get calendar by uri.
        return await this.GetCalendarWithUri(resource.Uri); ;
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

        // Query for data.
        var result = await this.client
            .Propfind(uri, propfind)
            .Send()
            .ConfigureAwait(false);

        if (!result.IsSuccessful)
        {
            return null;
        }

        // Get resource.
        var resource = result.Resources.FirstOrDefault();

        // Check if the resource really is a calendar.
        var contentType = resource?.Properties.FirstOrDefault(x => x.Key.LocalName == ElementNames.GetContentType);

        if (!contentType.HasValue || resource is null || !contentTypes.Contains(contentType.Value.Value))
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
                return this.tagRegex.Replace(keyValue.Value, string.Empty);
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
                    calendar.LastModified = DateTimeOffset.Parse(property.Value);
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

        calendar.Uid = uri
            .Replace(calendar.Owner, string.Empty)
            .Replace("/", string.Empty);

        // Fetch the events.
        var events = await this.GetEvents(uri).ConfigureAwait(false);
        calendar.Events = events.ToList();
        return calendar;
    }

    /// <summary>
    /// Gets the events.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="CalendarEvent"/>s.</returns>
    private async Task<IEnumerable<CalendarEvent>> GetEvents(string uri)
    {
        // Create the request body.
        var query = new XElement(Namespaces.CalNs + ElementNames.CalendarQuery, new XAttribute(XNamespace.Xmlns + ElementNames.D, Namespaces.DavNs), new XAttribute(XNamespace.Xmlns + ElementNames.C, Namespaces.CalNs));

        var prop = new XElement(Namespaces.DavNs + ElementNames.Prop);
        prop.Add(new XElement(Namespaces.DavNs + ElementNames.GetETag));
        prop.Add(new XElement(Namespaces.CalNs + ElementNames.CalendarData));
        query.Add(prop);

        var filter = new XElement(Namespaces.CalNs + ElementNames.Filter);
        filter.Add(new XElement(Namespaces.CalNs + ElementNames.CompFilter, new XAttribute(ElementNames.Name, ElementNames.VCalendar)));
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
