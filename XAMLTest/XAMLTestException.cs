using System.Runtime.Serialization;

namespace XamlTest;

public class XamlTestException : Exception
{
    public XamlTestException()
    { }

    public XamlTestException(string? message) 
        : base(message)
    { }

    public XamlTestException(string? message, Exception? innerException) 
        : base(message, innerException)
    { }

    protected XamlTestException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    { }
}
