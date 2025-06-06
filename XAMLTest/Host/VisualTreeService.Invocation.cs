using Grpc.Core;

namespace XamlTest.Host;

partial class VisualTreeService
{
    public override async Task<RemoteInvocationResult> RemoteInvocation(RemoteInvocationRequest request, ServerCallContext context)
    {
        RemoteInvocationResult reply = new();
        await Application.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                object? element = null;
                if (request.UseAppAsElement)
                {
                    element = Application;
                }
                else
                {
                    element = GetCachedElement<DependencyObject>(request.ElementId);
                }
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
                            var parameters = new object?[request.Parameters.Count + 1];
                            parameters[0] = element;
                            var methodParameters = method.GetParameters();
                            if (methodParameters.Length == parameters.Length)
                            {
                                for (int i = 0; i < request.Parameters.Count; i++)
                                {
                                    Type parameterType = methodParameters[i + 1].ParameterType;
                                    parameters[i + 1] = Serializer.Deserialize(parameterType, request.Parameters[i]);
                                }

                                if (request.MethodGenericTypes.Any())
                                {
                                    Type[] genericTypes = request.MethodGenericTypes.Select(x => Type.GetType(x, true)!).ToArray();
                                    method = method.MakeGenericMethod(genericTypes);
                                }

                                object? response = method.Invoke(null, parameters);
                                reply.ValueType = method.ReturnType.AssemblyQualifiedName;
                                if (method.ReturnType != typeof(void))
                                {
                                    reply.Value = Serializer.Serialize(method.ReturnType, response);
                                }
                            }
                            else
                            {
                                reply.ErrorMessages.Add($"{request.MethodContainerType}.{request.MethodName} contains {methodParameters.Length} does not match the number of passed parameters {parameters.Length}");
                            }
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
        return reply;
    }
}
