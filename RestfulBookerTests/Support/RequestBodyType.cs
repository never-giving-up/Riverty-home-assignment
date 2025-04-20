using System;
using System.Collections.Generic;

namespace RestfulBookerTests.Support;

public enum RequestBodyType
{
    Json,
    Xml,
    UrlEncodedForm
}

public static class RequestBodyTypeHelper
{
    private static readonly Dictionary<string, RequestBodyType> StringToRequestBodyTypeMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "json", RequestBodyType.Json },
            { "xml", RequestBodyType.Xml },
            { "urlencoded", RequestBodyType.UrlEncodedForm }
        };

    public static RequestBodyType FromString(string input)
    {
        return StringToRequestBodyTypeMap.TryGetValue(input, out var result) ? result : throw new ArgumentException($"Unknown request body type: {input}");
    }
}

