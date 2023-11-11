// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Namespaces.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The namespace constants.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Constants;

/// <summary>
/// The namespace constants.
/// </summary>
public static class Namespaces
{
    /// <summary>
    /// The DAV namespace.
    /// </summary>
    public static readonly XNamespace DavNs = "DAV:";

    /// <summary>
    /// The CalDAV namespace.
    /// </summary>
    public static readonly XNamespace CalNs = "urn:ietf:params:xml:ns:caldav";

    /// <summary>
    /// The server namespace.
    /// </summary>
    public static readonly XNamespace ServerNs = "http://calendarserver.org/ns/";

    /// <summary>
    /// The ICal namespace.
    /// </summary>
    public static readonly XNamespace ICalNs = "http://apple.com/ns/ical/";
}
