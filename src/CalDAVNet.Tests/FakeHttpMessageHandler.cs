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
    /// The function that builds the response out of the request and its body.
    /// </summary>
    private readonly Func<HttpRequestMessage, string, HttpResponseMessage> answer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeHttpMessageHandler"/> class.
    /// </summary>
    /// <param name="response">The response to answer with.</param>
    public FakeHttpMessageHandler(HttpResponseMessage response)
    {
        this.answer = (_, _) => response;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeHttpMessageHandler"/> class that answers every request
    /// with its own response, the way a server would.
    /// </summary>
    /// <param name="answer">The function that builds the response out of the request and its body.</param>
    public FakeHttpMessageHandler(Func<HttpRequestMessage, string, HttpResponseMessage> answer)
    {
        this.answer = answer;
    }

    /// <summary>
    /// Gets the request the handler was called with last.
    /// </summary>
    public HttpRequestMessage? SentRequest { get; private set; }

    /// <summary>
    /// Gets the bodies of all requests the handler was called with, in their order.
    /// </summary>
    public List<string> SentBodies { get; } = [];

    /// <inheritdoc cref="HttpMessageHandler"/>
    /// <summary>
    /// Answers the request with the canned response or the response of the answer function.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
    /// <seealso cref="HttpMessageHandler"/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        this.SentRequest = request;
        this.SentBodies.Add(body);

        var response = this.answer(request, body);
        response.RequestMessage = request;
        return response;
    }
}
