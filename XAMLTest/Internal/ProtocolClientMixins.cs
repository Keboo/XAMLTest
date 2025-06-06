using XamlTest.Host;

namespace XamlTest.Internal;

internal static class ProtocolClientMixins
{
    public static async Task<TReturn?> RemoteExecute<TReturn>(this Protocol.ProtocolClient client,
        Serializer serializer,
        Action<string>? logMessage,
        Action<RemoteInvocationRequest> updateRequest,
        Delegate @delegate,
        object?[] parameters)
    {
        if (@delegate.Target is not null)
        {
            throw new ArgumentException("Cannot execute a non-static delegate remotely");
        }
        if (@delegate.Method.DeclaringType is null)
        {
            throw new ArgumentException("Could not find containing type for delegate");
        }

        var request = new RemoteInvocationRequest()
        {
            MethodName = @delegate.Method.Name,
            MethodContainerType = @delegate.Method.DeclaringType!.AssemblyQualifiedName,
            Assembly = @delegate.Method.DeclaringType.Assembly.FullName,
        };
        foreach (var parameter in parameters)
        {
            request.Parameters.Add(serializer.Serialize(parameter?.GetType() ?? typeof(object), parameter));
        }
        if (@delegate.Method.IsGenericMethod)
        {
            foreach (var genericArguments in @delegate.Method.GetGenericArguments())
            {
                request.MethodGenericTypes.Add(genericArguments.AssemblyQualifiedName);
            }
        }
        updateRequest(request);
        logMessage?.Invoke($"{nameof(RemoteExecute)}({request})");
        if (await client.RemoteInvocationAsync(request) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }

            if (reply.ValueType is null)
            {
                return default;
            }

            if (reply.Value is TReturn converted && typeof(TReturn) != typeof(string))
            {
                return converted;
            }

            return (TReturn)serializer.Deserialize(typeof(TReturn), reply.Value ?? "")!;
        }
        return default;
    }
}
