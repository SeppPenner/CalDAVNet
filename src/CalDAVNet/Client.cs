// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Client.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The client class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet;

/// <summary>
/// The client class.
/// </summary>
public partial class Client
{
    /// <summary>
    /// The content types.
    /// </summary>
    private static readonly List<string> contentTypes = new()
    {
        "httpd/unix-directory",
        "text/calendar"
    };

    /// <summary>
    /// The tag regex.
    /// </summary>
    [GeneratedRegex("<[^>]*(>|$)")]
    private static partial Regex TagRegex();

    /// <summary>
    /// The tag regex.
    /// </summary>
    private readonly Regex tagRegex = TagRegex();

    /// <summary>
    /// The CalDAV client.
    /// </summary>
    private readonly CalDAVClient client;

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <param name="password">The password.</param>
    /// <param name="userName">The user name.</param>
    public Client(Uri uri, string userName, string password)
    {
        this.Uri = uri;
        this.UserName = userName;
        this.client = new CalDAVClient(uri, userName, password);
    }

    /// <summary>
    /// Gets the user name to authenticate with.
    /// </summary>
    public string UserName { get; } = string.Empty;

    /// <summary>
    /// Gets the uri of the server to connect to.
    /// </summary>
    public Uri Uri { get; }
}
