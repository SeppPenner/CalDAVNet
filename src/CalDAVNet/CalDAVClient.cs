// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Client.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The CalDAV client class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet;

/// <summary>
/// The CalDAV client class.
/// </summary>
internal class CalDAVClient
{
    /// <summary>
    /// The HTTP client.
    /// </summary>
    private static readonly HttpClient client = new();

    /// <summary>
    /// The propfind HTTP method.
    /// </summary>
    private static readonly HttpMethod propfindMethod = new(HttpMethods.PropFind);

    /// <summary>
    /// The report HTTP method.
    /// </summary>
    private static readonly HttpMethod reportMethod = new(HttpMethods.Report);

    /// <summary>
    /// The base uri.
    /// </summary>
    private readonly Uri baseUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalDAVClient"/>.
    /// </summary>
    /// <param name="baseUri">The base uri.</param>
    /// <param name="userName">The user name.</param>
    /// <param name="password">The password.</param>
    public CalDAVClient(Uri baseUri, string userName, string password)
    {
        this.baseUri = baseUri;
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Xml));
        client.DefaultRequestHeaders.Add(HeaderNames.Prefer, HeaderValues.ReturnMinimal);
        client.DefaultRequestHeaders.Add(HeaderNames.Depth, HeaderValues.One);
        var value = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(HeaderNames.Basic, value);
    }

    /// <summary>
    /// Gets data from an uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>The <see cref="Request{T}"/> of <see cref="ResourceResponse"/>.</returns>
    public Request<ResourceResponse> Get(string uri)
    {
        return this.CreateRequest<ResourceResponse>(uri)
            .WithMethod(HttpMethod.Get);
    }

    /// <summary>
    /// Updates data to an uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <param name="content">The content.</param>
    /// <returns>The <see cref="Request{T}"/> of <see cref="Response"/>.</returns>
    public Request<Response> Put(string uri, string content)
    {
        return this.CreateRequest<Response>(uri)
            .WithMethod(HttpMethod.Put)
            .WithContent(new StringContent(content));
    }

    /// <summary>
    /// Deletes data from an uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>The <see cref="Request{T}"/> of <see cref="Response"/>.</returns>
    public Request<Response> Delete(string uri)
    {
        return this.CreateRequest<Response>(uri)
            .WithMethod(HttpMethod.Delete);
    }

    /// <summary>
    /// Does a propfind HTTP request from an uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <param name="root">The XML root element.</param>
    /// <returns>The <see cref="Request{T}"/> of <see cref="ResourceResponse"/>.</returns>
    public Request<ResourceResponse> Propfind(string uri, XElement root)
    {
        return this.CreateRequest<ResourceResponse>(uri)
            .WithMethod(propfindMethod)
            .WithXmlContent(root);
    }

    /// <summary>
    /// Does a report HTTP request from an uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <param name="root">The XML root element.</param>
    /// <returns>The <see cref="Request{T}"/> of <see cref="ResourceResponse"/>.</returns>
    public Request<ResourceResponse> Report(string uri, XElement root)
    {
        return this.CreateRequest<ResourceResponse>(uri)
            .WithMethod(reportMethod)
            .WithXmlContent(root);
    }

    /// <summary>
    /// Creates a request.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="uri">The uri.</param>
    /// <returns>The <see cref="Request{T}"/>.</returns>
    private Request<T> CreateRequest<T>(string uri) where T : Response, new()
    {
        return new Request<T>(new Uri(this.baseUri, uri), client);
    }
}
