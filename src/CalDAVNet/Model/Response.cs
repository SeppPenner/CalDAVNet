// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Response.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The response class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Model;

/// <summary>
/// The response class.
/// </summary>
internal class Response
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Response"/> class.
    /// </summary>
    public Response()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Response"/> class.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="statusCode">The status code.</param>
    public Response(string method, int statusCode)
    {
        this.Method = method;
        this.StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the status code.
    /// </summary>
    public int StatusCode { get; private set; }

    /// <summary>
    /// Gets the HTTP method.
    /// </summary>
    public string Method { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the request was successful or not.
    /// </summary>
    public virtual bool IsSuccessful => this.StatusCode >= 200 && this.StatusCode <= 299;

    /// <summary>
    /// Parses the HTTP response message.
    /// </summary>
    /// <param name="message">The HTTP response message.</param>
    public virtual Task Parse(HttpResponseMessage message)
    {
        this.Method = message.RequestMessage?.Method.Method ?? string.Empty;
        this.StatusCode = (int)message.StatusCode;
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="object"/>
    public override string ToString()
    {
        return $"{this.Method} CalDAV response - Status code: {this.StatusCode}";
    }

    /// <summary>
    /// Gets the encoding of the HTTP content.
    /// </summary>
    /// <param name="content">The HTTP content.</param>
    /// <param name="defaultEncoding">The default encoding.</param>
    /// <returns>The <see cref="Encoding"/>.</returns>
    protected static Encoding GetEncoding(HttpContent content, Encoding defaultEncoding)
    {
        if (content.Headers.ContentType?.CharSet is null)
        {
            return defaultEncoding;
        }

        try
        {
            return Encoding.GetEncoding(content.Headers.ContentType.CharSet);
        }
        catch
        {
            return defaultEncoding;
        }
    }
}
