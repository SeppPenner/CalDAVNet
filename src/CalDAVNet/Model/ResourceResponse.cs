// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceResponse.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The resource response class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Model;

/// <inheritdoc cref="Response"/>
/// <summary>
/// The resource response class.
/// </summary>
internal class ResourceResponse : Response
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceResponse"/> class.
    /// </summary>
    public ResourceResponse()
    {
        this.Resources = new List<Resource>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceResponse"/> class.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="resources">The resources.</param>
    public ResourceResponse(string method, int statusCode, IReadOnlyCollection<Resource> resources) : base(method, statusCode)
    {
        this.Resources = resources;
    }

    /// <summary>
    /// Gets the resources.
    /// </summary>
    public IReadOnlyCollection<Resource> Resources { get; private set; }

    /// <inheritdoc cref="Response"/>
    public override async Task Parse(HttpResponseMessage message)
    {
        await base.Parse(message);

        var data = await message.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        var content = GetEncoding(message.Content, Encoding.UTF8).GetString(data, 0, data.Length);

        if (TryParseDocument(content, out var document) == false || document.Root is null)
        {
            return;
        }

        this.Resources = document.Root
            .LocalNameElements(ElementNames.Response)
            .Select(ParseResource)
            .ToList();
    }

    /// <summary>
    /// Parses the resource.
    /// </summary>
    /// <param name="element">The XML element.</param>
    /// <returns>The <see cref="Resource"/>.</returns>
    private static Resource ParseResource(XElement element)
    {
        var uri = element.LocalNameElement(ElementNames.Href)?.Value ?? string.Empty;
        var status = element.LocalNameElement(ElementNames.Status)?.Value ?? string.Empty;

        var properties = element
            .LocalNameElements(ElementNames.PropStat)
            .Where(x =>
            {
                var statusCode = x.GetStatusCode();
                return statusCode >= 200 && statusCode <= 299;
            })
            .SelectMany(x => x.LocalNameElements(ElementNames.Prop).Elements())
            .Select(x => new KeyValuePair<XName, string>(x.Name, x.GetInnerXml()))
            .ToDictionary(x => x.Key, x => x.Value);

        return new Resource
        {
            Uri = uri,
            Status = status,
            Properties = properties
        };
    }

    /// <summary>
    /// Tries to parse the document.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="document">The document.</param>
    /// <returns>A value indicating whether the document could be parsed or not.</returns>
    private static bool TryParseDocument(string text, [NotNullWhen(true)] out XDocument? document)
    {
        try
        {
            document = XDocument.Parse(text);
            return true;
        }
        catch
        {
            document = null;
        }

        return false;
    }
}
