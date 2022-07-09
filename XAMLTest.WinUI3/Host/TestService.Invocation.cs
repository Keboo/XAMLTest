namespace XamlTest.Host;

partial class TestService
{
    protected override async Task<RemoteInvocationResult> RemoteInvocation(RemoteInvocationRequest request)
    {
        RemoteInvocationResult reply = new();
        bool success = await Dispatcher.TryInvokeAsync(() =>
        {
            try
            {
                DependencyObject? element = GetCachedElement<DependencyObject>(request.ElementId);
                if (element is null)
                {
                    reply.ErrorMessages.Add("Failed to find element to execute remote code");
                }
                Assembly? assembly = LoadedAssemblies.FirstOrDefault(x => x.GetName().FullName == request.Assembly);
                if (assembly is null)
                {
                    reply.ErrorMessages.Add($"Failed to find assembly '{request.Assembly}' for remote code");
                }
                else
                {
                    if (Type.GetType(request.MethodContainerType) is { } containingType)
                    {
                        if (containingType.GetMethod(request.MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy) is { } method)
                        {
                            method.Invoke(null, new object?[] { element });
                        }
                        else
                        {
                            reply.ErrorMessages.Add($"Could not find method '{request.MethodName}' on {containingType.FullName}");
                        }
                    }
                    else
                    {
                        reply.ErrorMessages.Add($"Could not find method containing type '{request.MethodContainerType}'");
                    }
                }
            }
            catch (Exception e)
            {
                reply.ErrorMessages.Add(e.ToString());
            }
        });
        if (!success)
        {
            reply.ErrorMessages.Add($"Failed to process {nameof(RemoteInvocation)}");
        }
        return reply;
    }
}
