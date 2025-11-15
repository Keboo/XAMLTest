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
}
