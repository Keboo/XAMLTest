using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace XamlTest
{
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
}
