// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="Request{T}" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Tests;

/// <summary>
/// A class to test the <see cref="Request{T}"/> class.
/// </summary>
[TestClass]
public class RequestTests
{
    /// <summary>
    /// Checks whether a header is replaced instead of gaining a second value.
    /// </summary>
    [TestMethod]
    public void WithHeaderReplacesAHeaderOfTheSameName()
    {
        var request = new Request<Response>(new Uri(TestDataProvider.BaseUri), new HttpClient());

        request
            .WithHeader(HeaderNames.Depth, HeaderValues.One)
            .WithHeader(HeaderNames.Depth, HeaderValues.Zero);

        CollectionAssert.AreEqual(new[] { HeaderValues.Zero }, request.Headers.GetValues(HeaderNames.Depth).ToArray());
    }

    /// <summary>
    /// Checks whether the XML content is sent as application/xml.
    /// </summary>
    [TestMethod]
    public async Task WithXmlContentSendsTheRootElementAsXml()
    {
        var request = new Request<Response>(new Uri(TestDataProvider.BaseUri), new HttpClient());
        var root = new XElement(Namespaces.DavNs + ElementNames.PropFind, new XElement(Namespaces.DavNs + ElementNames.AllProps));

        request.WithXmlContent(root);

        Assert.IsNotNull(request.Content);
        Assert.AreEqual("application/xml", request.Content.Headers.ContentType?.MediaType);
        StringAssert.Contains(await request.Content.ReadAsStringAsync(), "<allprop");
    }

    /// <summary>
    /// Checks whether the XML declaration that <see cref="Request{T}.WithXmlContent"/> builds really is dropped
    /// again. <see cref="XDocument.ToString()"/> never writes a declaration, so the body starts with the root
    /// element. That is existing behaviour, servers accept it.
    /// </summary>
    [TestMethod]
    public async Task WithXmlContentDropsTheXmlDeclaration()
    {
        var request = new Request<Response>(new Uri(TestDataProvider.BaseUri), new HttpClient());
        var root = new XElement(Namespaces.DavNs + ElementNames.PropFind);

        request.WithXmlContent(root);

        Assert.IsNotNull(request.Content);
        StringAssert.StartsWith(await request.Content.ReadAsStringAsync(), "<propfind");
    }

    /// <summary>
    /// Checks whether sending a request parses the status code and the method out of the answer.
    /// </summary>
    [TestMethod]
    public async Task SendParsesTheStatusCodeAndTheMethodOfTheAnswer()
    {
        var handler = new FakeHttpMessageHandler(TestDataProvider.GetResponseMessage(HttpStatusCode.MultiStatus, TestDataProvider.GetMultiStatusResponse()));
        var request = new Request<ResourceResponse>(new Uri(TestDataProvider.BaseUri), new HttpClient(handler))
            .WithMethod(new HttpMethod(HttpMethods.PropFind));

        var response = await request.Send();

        Assert.AreEqual(207, response.StatusCode);
        Assert.AreEqual(HttpMethods.PropFind, response.Method);
        Assert.IsTrue(response.IsSuccessful);
        Assert.AreEqual(1, response.Resources.Count);
    }

    /// <summary>
    /// Checks whether the request that goes out carries the headers that were set on it.
    /// </summary>
    [TestMethod]
    public async Task SendUsesTheHeadersOfTheRequest()
    {
        var handler = new FakeHttpMessageHandler(TestDataProvider.GetResponseMessage(HttpStatusCode.OK, string.Empty));
        var request = new Request<Response>(new Uri(TestDataProvider.BaseUri), new HttpClient(handler))
            .WithMethod(HttpMethod.Get)
            .WithHeader(HeaderNames.Depth, HeaderValues.Zero);

        await request.Send();

        Assert.IsNotNull(handler.SentRequest);
        CollectionAssert.AreEqual(new[] { HeaderValues.Zero }, handler.SentRequest.Headers.GetValues(HeaderNames.Depth).ToArray());
    }
}
