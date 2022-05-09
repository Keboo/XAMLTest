using System;
using System.Runtime.Serialization;

namespace XamlTest;

public class XAMLTestException : Exception
{
    public XAMLTestException()
    { }

    public XAMLTestException(string? message) 
        : base(message)
    { }

    public XAMLTestException(string? message, Exception? innerException) 
        : base(message, innerException)
    { }

    protected XAMLTestException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    { }
}
