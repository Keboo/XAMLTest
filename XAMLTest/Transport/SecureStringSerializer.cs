using System.Runtime.InteropServices;
using System.Security;

namespace XamlTest.Transport;

public class SecureStringSerializer : ISerializer
{
    public bool CanSerialize(Type type, ISerializer rootSerializer)
        => type == typeof(SecureString);

    public object? Deserialize(Type type, string value, ISerializer rootSerializer)
    {
        var rv = new SecureString();
        foreach(var c in value)
        {
            rv.AppendChar(c);
        }
        return rv;
    }

    public string Serialize(Type type, object? value, ISerializer rootSerializer)
    {
        if (value is SecureString secureString)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(valuePtr) ?? "";
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
        return "";
    }

}
