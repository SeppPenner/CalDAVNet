// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="Client" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Tests;

/// <summary>
/// A class to test the <see cref="Client"/> class. Everything that talks to a server needs one, so only the
/// parts that do not send a request are tested here.
/// </summary>
[TestClass]
public class ClientTests
{
    /// <summary>
    /// Checks whether both constructors end up with the same uri and user name.
    /// </summary>
    [TestMethod]
    public void BothConstructorsKeepTheUriAndTheUserName()
    {
        var fromString = new Client(TestDataProvider.BaseUri, TestDataProvider.UserName, TestDataProvider.Password);
        var fromUri = new Client(new Uri(TestDataProvider.BaseUri), TestDataProvider.UserName, TestDataProvider.Password);

        Assert.AreEqual(TestDataProvider.BaseUri, fromString.Uri.ToString());
        Assert.AreEqual(TestDataProvider.BaseUri, fromUri.Uri.ToString());
        Assert.AreEqual(TestDataProvider.UserName, fromString.UserName);
        Assert.AreEqual(TestDataProvider.UserName, fromUri.UserName);
    }

    /// <summary>
    /// Checks whether an empty uid is rejected before a request goes out.
    /// </summary>
    [TestMethod]
    public void GetCalendarByUidWithoutAUidThrowsAnArgumentNullException()
    {
        var client = new Client(TestDataProvider.BaseUri, TestDataProvider.UserName, TestDataProvider.Password);

        Assert.ThrowsExactly<ArgumentNullException>(() => client.GetCalendarByUid(string.Empty));
        Assert.ThrowsExactly<ArgumentNullException>(() => client.GetCalendarByUid("   "));
    }
}
