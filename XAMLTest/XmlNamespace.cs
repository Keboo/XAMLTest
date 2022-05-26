using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace XamlTest;

public sealed class XmlNamespace : IEquatable<XmlNamespace>
{
    public string? Prefix { get; }
    public string Uri { get; }

    public XmlNamespace(string? prefix, string uri)
    {
        if (string.IsNullOrEmpty(uri))
        {
            throw new ArgumentException($"'{nameof(uri)}' cannot be null or empty.", nameof(uri));
        }

        Prefix = prefix;
        Uri = uri;
    }

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(Prefix))
        {
            return $"xmlns=\"{Uri}\"";
        }
        return $"xmlns:{Prefix}=\"{Uri}\"";
    }

    public override int GetHashCode() => HashCode.Combine(Prefix, Uri);

    public static bool operator ==(XmlNamespace? left, XmlNamespace? right)
    {
        return EqualityComparer<XmlNamespace>.Default.Equals(left, right);
    }

    public static bool operator !=(XmlNamespace? left, XmlNamespace? right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as XmlNamespace);
    }

    public bool Equals([AllowNull] XmlNamespace other)
    {
        if (other is null) return false;
        return other.Prefix == Prefix && other.Uri == Uri;
    }
}