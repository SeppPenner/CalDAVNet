// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FakeHttpMessageHandler.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A HTTP message handler that answers with a canned response and remembers the request it got.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Tests;

/// <summary>
/// A HTTP message handler that answers with a canned response and remembers the request it got. It keeps the
/// tests away from the network, no test in this project opens a connection.
/// </summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    /// <summary>
    /// The response to answer with.
    /// </summary>
    private readonly HttpResponseMessage response;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeHttpMessageHandler"/> class.
    /// </summary>
    /// <param name="response">The response to answer with.</param>
    public FakeHttpMessageHandler(HttpResponseMessage response)
    {
        this.response = response;
    }

    /// <summary>
    /// Gets the request the handler was called with.
    /// </summary>
    public HttpRequestMessage? SentRequest { get; private set; }

    /// <inheritdoc cref="HttpMessageHandler"/>
    /// <summary>
    /// Answers the request with the canned response.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The canned <see cref="HttpResponseMessage"/>.</returns>
    /// <seealso cref="HttpMessageHandler"/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        this.SentRequest = request;
        this.response.RequestMessage = request;
        return Task.FromResult(this.response);
    }
}
