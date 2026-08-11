// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDataProvider.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to provide the test data used in the tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Tests;

/// <summary>
/// A class to provide the test data used in the tests.
/// </summary>
public static class TestDataProvider
{
    /// <summary>
    /// The user name used in the tests.
    /// </summary>
    public const string UserName = "user";

    /// <summary>
    /// The password used in the tests.
    /// </summary>
    public const string Password = "password";

    /// <summary>
    /// The base uri used in the tests. It is the address of the example project with a trailing slash.
    /// </summary>
    public const string BaseUri = "http://192.168.2.2/caldav.php/user/someid/";

    /// <summary>
    /// The uri of the calendar in <see cref="GetMultiStatusResponse"/>.
    /// </summary>
    public const string CalendarUri = "/caldav.php/user/someid/calendar/";

    /// <summary>
    /// Gets a multistatus response body as a CalDAV server answers it to a propfind request. The second
    /// propstat block carries a status of 404, its properties have to be dropped.
    /// </summary>
    /// <returns>The multistatus response body as <see cref="string"/>.</returns>
    public static string GetMultiStatusResponse()
    {
        return """
            <?xml version="1.0" encoding="utf-8"?>
            <D:multistatus xmlns:D="DAV:" xmlns:C="urn:ietf:params:xml:ns:caldav" xmlns:CS="http://calendarserver.org/ns/">
              <D:response>
                <D:href>/caldav.php/user/someid/calendar/</D:href>
                <D:propstat>
                  <D:prop>
                    <D:displayname>Test calendar</D:displayname>
                    <D:getcontenttype>text/calendar</D:getcontenttype>
                    <D:getlastmodified>Tue, 11 Aug 2026 10:00:00 GMT</D:getlastmodified>
                    <D:owner><D:href>/caldav.php/user/someid/</D:href></D:owner>
                  </D:prop>
                  <D:status>HTTP/1.1 200 OK</D:status>
                </D:propstat>
                <D:propstat>
                  <D:prop>
                    <CS:getctag />
                  </D:prop>
                  <D:status>HTTP/1.1 404 Not Found</D:status>
                </D:propstat>
              </D:response>
            </D:multistatus>
            """;
    }

    /// <summary>
    /// Gets a HTTP response message with the given status code and body.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <param name="content">The body.</param>
    /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
    public static HttpResponseMessage GetResponseMessage(HttpStatusCode statusCode, string content)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/xml")
        };
    }
}
