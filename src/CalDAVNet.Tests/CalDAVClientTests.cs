// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalDAVClientTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="CalDAVClient" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Tests;

/// <summary>
/// A class to test the <see cref="CalDAVClient"/> class. The tests only build requests, they never send one,
/// so no server is needed.
/// </summary>
[TestClass]
public class CalDAVClientTests
{
    /// <summary>
    /// Checks whether every request carries the accept, prefer and depth headers of the client.
    /// </summary>
    [TestMethod]
    public void EveryRequestCarriesTheDefaultHeaders()
    {
        var calDavClient = new CalDAVClient(new Uri(TestDataProvider.BaseUri), TestDataProvider.UserName, TestDataProvider.Password);

        var request = calDavClient.Get(string.Empty);

        Assert.AreEqual("application/xml", request.Headers.Accept.ToString());
        CollectionAssert.AreEqual(new[] { HeaderValues.ReturnMinimal }, request.Headers.GetValues(HeaderNames.Prefer).ToArray());
        CollectionAssert.AreEqual(new[] { HeaderValues.One }, request.Headers.GetValues(HeaderNames.Depth).ToArray());
    }

    /// <summary>
    /// Checks whether the credentials end up in the request as a basic authentication header.
    /// </summary>
    [TestMethod]
    public void EveryRequestCarriesTheCredentialsOfTheClient()
    {
        var calDavClient = new CalDAVClient(new Uri(TestDataProvider.BaseUri), TestDataProvider.UserName, TestDataProvider.Password);

        var request = calDavClient.Get(string.Empty);

        Assert.AreEqual(HeaderNames.Basic, request.Headers.Authorization?.Scheme);
        Assert.AreEqual($"{TestDataProvider.UserName}:{TestDataProvider.Password}", Decode(request.Headers.Authorization?.Parameter));
    }

    /// <summary>
    /// Checks whether a second client leaves the requests of the first one alone. The HTTP client is static, up
    /// to version 1.0.3.0 the credentials were written to its default headers, so the second client took over
    /// the requests of the first one and the header values piled up.
    /// </summary>
    [TestMethod]
    public void ASecondClientDoesNotTakeOverTheRequestsOfTheFirstOne()
    {
        var firstClient = new CalDAVClient(new Uri(TestDataProvider.BaseUri), "firstUser", "firstPassword");
        var requestBefore = firstClient.Get(string.Empty);

        var secondClient = new CalDAVClient(new Uri(TestDataProvider.BaseUri), "secondUser", "secondPassword");
        var secondRequest = secondClient.Get(string.Empty);
        var requestAfter = firstClient.Get(string.Empty);

        Assert.AreEqual("firstUser:firstPassword", Decode(requestBefore.Headers.Authorization?.Parameter));
        Assert.AreEqual("secondUser:secondPassword", Decode(secondRequest.Headers.Authorization?.Parameter));
        Assert.AreEqual("firstUser:firstPassword", Decode(requestAfter.Headers.Authorization?.Parameter));

        Assert.AreEqual(1, requestAfter.Headers.GetValues(HeaderNames.Depth).Count());
        Assert.AreEqual(1, requestAfter.Headers.GetValues(HeaderNames.Prefer).Count());
        Assert.AreEqual(1, requestAfter.Headers.Accept.Count);
    }

    /// <summary>
    /// Checks whether credentials outside of ASCII survive the encoding. Up to version 1.0.3.0 they were encoded
    /// as ASCII, which turned every umlaut into a question mark.
    /// </summary>
    [TestMethod]
    public void CredentialsOutsideOfAsciiAreEncodedAsUtf8()
    {
        var calDavClient = new CalDAVClient(new Uri(TestDataProvider.BaseUri), "bärbel", "geheimnis");

        var request = calDavClient.Get(string.Empty);

        Assert.AreEqual("bärbel:geheimnis", Decode(request.Headers.Authorization?.Parameter));
    }

    /// <summary>
    /// Checks whether a depth of zero replaces the default depth of one instead of being added to it. A second
    /// value would go out as "Depth: 1, 0".
    /// </summary>
    [TestMethod]
    public void ADepthOfZeroReplacesTheDefaultDepth()
    {
        var calDavClient = new CalDAVClient(new Uri(TestDataProvider.BaseUri), TestDataProvider.UserName, TestDataProvider.Password);

        var request = calDavClient
            .Propfind(string.Empty, new XElement(Namespaces.DavNs + ElementNames.PropFind))
            .WithHeader(HeaderNames.Depth, HeaderValues.Zero);

        CollectionAssert.AreEqual(new[] { HeaderValues.Zero }, request.Headers.GetValues(HeaderNames.Depth).ToArray());
    }

    /// <summary>
    /// Checks whether the relative uri of a request is combined with the base uri of the client.
    /// </summary>
    [TestMethod]
    public void TheRelativeUriIsCombinedWithTheBaseUri()
    {
        var calDavClient = new CalDAVClient(new Uri(TestDataProvider.BaseUri), TestDataProvider.UserName, TestDataProvider.Password);

        var request = calDavClient.Get("calendar/");

        Assert.AreEqual($"{TestDataProvider.BaseUri}calendar/", request.Uri?.ToString());
    }

    /// <summary>
    /// Checks whether every method of the client uses the HTTP method it stands for.
    /// </summary>
    [TestMethod]
    public void EveryMethodUsesItsOwnHttpMethod()
    {
        var calDavClient = new CalDAVClient(new Uri(TestDataProvider.BaseUri), TestDataProvider.UserName, TestDataProvider.Password);
        var root = new XElement(Namespaces.DavNs + ElementNames.PropFind);

        Assert.AreEqual(HttpMethod.Get, calDavClient.Get(string.Empty).Method);
        Assert.AreEqual(HttpMethod.Put, calDavClient.Put(string.Empty, "content").Method);
        Assert.AreEqual(HttpMethod.Delete, calDavClient.Delete(string.Empty).Method);
        Assert.AreEqual(HttpMethods.PropFind, calDavClient.Propfind(string.Empty, root).Method.Method);
        Assert.AreEqual(HttpMethods.Report, calDavClient.Report(string.Empty, root).Method.Method);
    }

    /// <summary>
    /// Checks whether the content of a put request is the string that was handed in.
    /// </summary>
    [TestMethod]
    public async Task ThePutContentIsTheGivenString()
    {
        var calDavClient = new CalDAVClient(new Uri(TestDataProvider.BaseUri), TestDataProvider.UserName, TestDataProvider.Password);

        var request = calDavClient.Put(string.Empty, "BEGIN:VCALENDAR");

        Assert.IsNotNull(request.Content);
        Assert.AreEqual("BEGIN:VCALENDAR", await request.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Decodes the parameter of a basic authentication header.
    /// </summary>
    /// <param name="parameter">The parameter of the authentication header.</param>
    /// <returns>The decoded credentials as <see cref="string"/>.</returns>
    private static string Decode(string? parameter)
    {
        return parameter is null ? string.Empty : Encoding.UTF8.GetString(Convert.FromBase64String(parameter));
    }
}
