// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Request.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The request class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Model;

/// <summary>
/// The request class.
/// </summary>
/// <typeparam name="T">The type parameter of the response.</typeparam>
internal class Request<T> where T : Response, new()
{
    /// <summary>
    /// The HTTP client.
    /// </summary>
    private readonly HttpClient client;

    /// <summary>
    /// The request message.
    /// </summary>
    private readonly HttpRequestMessage requestMessage = new();

    /// <summary>
    /// Initializes a new instance of the 
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="client"></param>
    public Request(Uri uri, HttpClient client)
    {
        this.requestMessage.RequestUri = uri;
        this.client = client;
    }

    /// <summary>
    /// Gets the response.
    /// </summary>
    public T Response { get; private set; } = new();

    /// <summary>
    /// Gets the headers.
    /// </summary>
    public HttpRequestHeaders Headers => this.requestMessage.Headers;

    /// <summary>
    /// Gets or sets the method.
    /// </summary>
    public HttpMethod Method
    {
        get => this.requestMessage.Method;
        private set => this.requestMessage.Method = value;
    }

    /// <summary>
    /// Gets or sets the HTTP content.
    /// </summary>
    public HttpContent? Content
    {
        get => this.requestMessage?.Content;
        private set => this.requestMessage.Content = value;
    }
    
    /// <summary>
    /// Sends a request message.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A response of type <see cref="T"/>.</returns>
    public async Task<T> Send(CancellationToken cancellationToken = default)
    {
        var message = await this.client.SendAsync(this.requestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
        this.Response = new T();
        await this.Response.Parse(message).ConfigureAwait(false);
        return this.Response;
    }

    /// <summary>
    /// Adds the HTTP method.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <returns>A <see cref="Request{T}"/>.</returns>
    public Request<T> WithMethod(HttpMethod method)
    {
        this.Method = method;
        return this;
    }

    /// <summary>
    /// Adds the HTTP content.
    /// </summary>
    /// <param name="content">The HTTP content.</param>
    /// <returns>A <see cref="Request{T}"/>.</returns>
    public Request<T> WithContent(HttpContent content)
    {
        this.Content = content;
        return this;
    }

    /// <summary>
    /// Adds the XML root element.
    /// </summary>
    /// <param name="root">The XML root element.</param>
    /// <returns>A <see cref="Request{T}"/>.</returns>
    public Request<T> WithXmlContent(XElement root)
    {
        var document = new XDocument(new XDeclaration(DocumentConstants.OnePointZero, DocumentConstants.Utf8, null));
        document.Add(root);

        return this.WithContent(document.ToStringContent());
    }

    /// <summary>
    /// Adds a header.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="name">The header value.</param>
    /// <returns>A <see cref="Request{T}"/>.</returns>
    public Request<T> WithHeader(string name, string value)
    {
        this.Headers.Add(name, value);
        return this;
    }
}
