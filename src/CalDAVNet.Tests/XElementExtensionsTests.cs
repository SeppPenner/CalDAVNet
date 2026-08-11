// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XElementExtensionsTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="XElementExtensions" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Tests;

/// <summary>
/// A class to test the <see cref="XElementExtensions"/> class.
/// </summary>
[TestClass]
public class XElementExtensionsTests
{
    /// <summary>
    /// Checks whether the local name lookup ignores the namespace and the case. Servers use different prefixes
    /// and different spellings for the same element, that is why the lookup works on the local name.
    /// </summary>
    [TestMethod]
    public void LocalNameElementIgnoresTheNamespaceAndTheCase()
    {
        var element = XElement.Parse("""<D:response xmlns:D="DAV:"><D:HREF>/calendar/</D:HREF></D:response>""");

        Assert.AreEqual("/calendar/", element.LocalNameElement(ElementNames.Href)?.Value);
    }

    /// <summary>
    /// Checks whether the local name lookup only sees the direct children of the element.
    /// </summary>
    [TestMethod]
    public void LocalNameElementsReturnsTheDirectChildrenOnly()
    {
        var element = XElement.Parse("<multistatus><response /><response><response /></response></multistatus>");

        Assert.AreEqual(2, element.LocalNameElements(ElementNames.Response).Count());
    }

    /// <summary>
    /// Checks whether the inner XML of an element is returned as markup and not as text.
    /// </summary>
    [TestMethod]
    public void GetInnerXmlReturnsTheMarkupBelowTheElement()
    {
        var element = XElement.Parse("<owner><href>/user/</href></owner>");

        Assert.AreEqual("<href>/user/</href>", element.GetInnerXml());
    }

    /// <summary>
    /// Checks whether the status code is read out of a status line.
    /// </summary>
    [TestMethod]
    public void GetStatusCodeReadsTheCodeOutOfTheStatusLine()
    {
        var element = XElement.Parse("<propstat><status>HTTP/1.1 404 Not Found</status></propstat>");

        Assert.AreEqual(404, element.GetStatusCode());
    }

    /// <summary>
    /// Checks whether a missing status and a status without a three digit group both end up as minus one.
    /// </summary>
    [TestMethod]
    public void GetStatusCodeWithoutACodeReturnsMinusOne()
    {
        Assert.AreEqual(-1, XElement.Parse("<propstat><prop /></propstat>").GetStatusCode());
        Assert.AreEqual(-1, XElement.Parse("<propstat><status>  </status></propstat>").GetStatusCode());
        Assert.AreEqual(-1, XElement.Parse("<propstat><status>HTTP/1.1 OK</status></propstat>").GetStatusCode());
    }

    /// <summary>
    /// Checks whether the description prefers the response description, falls back to the status and ends up
    /// empty if the element has neither.
    /// </summary>
    [TestMethod]
    public void GetDescriptionPrefersTheResponseDescription()
    {
        var withDescription = XElement.Parse("<response><responsedescription>Too much</responsedescription><status>HTTP/1.1 507</status></response>");
        var withStatus = XElement.Parse("<response><status>HTTP/1.1 507</status></response>");
        var withNeither = XElement.Parse("<response />");

        Assert.AreEqual("Too much", withDescription.GetDescription());
        Assert.AreEqual("HTTP/1.1 507", withStatus.GetDescription());
        Assert.AreEqual(string.Empty, withNeither.GetDescription());
    }
}
