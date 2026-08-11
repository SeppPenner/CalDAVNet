// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceResponseTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="ResourceResponse" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Tests;

/// <summary>
/// A class to test the <see cref="ResourceResponse"/> class.
/// </summary>
[TestClass]
public class ResourceResponseTests
{
    /// <summary>
    /// Checks whether the resource of a multistatus answer is read with its uri, its status and its properties.
    /// </summary>
    [TestMethod]
    public async Task ParseReadsTheResourceOfAMultiStatusAnswer()
    {
        var response = new ResourceResponse();

        await response.Parse(TestDataProvider.GetResponseMessage(HttpStatusCode.MultiStatus, TestDataProvider.GetMultiStatusResponse()));

        var resource = response.Resources.Single();
        Assert.AreEqual(TestDataProvider.CalendarUri, resource.Uri);
        Assert.AreEqual(string.Empty, resource.Status);
        Assert.AreEqual("Test calendar", resource.Properties.Single(x => x.Key.LocalName == ElementNames.DisplayName).Value);
        Assert.AreEqual("text/calendar", resource.Properties.Single(x => x.Key.LocalName == ElementNames.GetContentType).Value);
    }

    /// <summary>
    /// Checks whether the properties of a propstat block that did not answer with a 2xx status are dropped.
    /// </summary>
    [TestMethod]
    public async Task ParseDropsThePropertiesOfAFailedPropStat()
    {
        var response = new ResourceResponse();

        await response.Parse(TestDataProvider.GetResponseMessage(HttpStatusCode.MultiStatus, TestDataProvider.GetMultiStatusResponse()));

        var resource = response.Resources.Single();
        Assert.IsFalse(resource.Properties.Any(x => x.Key.LocalName == ElementNames.GetCTag));
    }

    /// <summary>
    /// Checks whether the value of a property is its inner XML and not its text. The owner is an example, it
    /// holds a href element, which is why the client strips the tags with a regex afterwards.
    /// </summary>
    [TestMethod]
    public async Task ParseReturnsTheInnerXmlOfAProperty()
    {
        var response = new ResourceResponse();

        await response.Parse(TestDataProvider.GetResponseMessage(HttpStatusCode.MultiStatus, TestDataProvider.GetMultiStatusResponse()));

        var owner = response.Resources.Single().Properties.Single(x => x.Key.LocalName == ElementNames.Owner).Value;
        StringAssert.Contains(owner, "href");
        StringAssert.Contains(owner, "/caldav.php/user/someid/");
    }

    /// <summary>
    /// Checks whether a body that is no XML at all leaves the response without resources instead of throwing.
    /// </summary>
    [TestMethod]
    public async Task ParseWithABodyThatIsNoXmlReturnsNoResources()
    {
        var response = new ResourceResponse();

        await response.Parse(TestDataProvider.GetResponseMessage(HttpStatusCode.OK, "This is not XML."));

        Assert.AreEqual(0, response.Resources.Count);
        Assert.AreEqual(200, response.StatusCode);
    }

    /// <summary>
    /// Checks whether the status code decides about the success of a response.
    /// </summary>
    [TestMethod]
    public async Task OnlyATwoHundredStatusCodeIsSuccessful()
    {
        var successful = new Response();
        var failed = new Response();

        await successful.Parse(TestDataProvider.GetResponseMessage(HttpStatusCode.MultiStatus, string.Empty));
        await failed.Parse(TestDataProvider.GetResponseMessage(HttpStatusCode.Unauthorized, string.Empty));

        Assert.IsTrue(successful.IsSuccessful);
        Assert.IsFalse(failed.IsSuccessful);
        Assert.AreEqual("401", failed.StatusCode.ToString());
    }

    /// <summary>
    /// Checks whether the string representation names the method and the status code.
    /// </summary>
    [TestMethod]
    public async Task ToStringNamesTheMethodAndTheStatusCode()
    {
        var message = TestDataProvider.GetResponseMessage(HttpStatusCode.NotFound, string.Empty);
        message.RequestMessage = new HttpRequestMessage(HttpMethod.Get, TestDataProvider.BaseUri);
        var response = new Response();

        await response.Parse(message);

        Assert.AreEqual("GET CalDAV response - Status code: 404", response.ToString());
    }
}
