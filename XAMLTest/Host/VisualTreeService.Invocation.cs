﻿using Grpc.Core;

namespace XamlTest.Host;

partial class VisualTreeService
{
    public override async Task<RemoteInvocationResult> RemoteInvocation(RemoteInvocationRequest request, ServerCallContext context)
    {
        RemoteInvocationResult reply = new();
        CultureInfo current = null!, uiCulture = null!;
        Application.Dispatcher.Invoke(() =>
        {
            current = Thread.CurrentThread.CurrentCulture;
            uiCulture = Thread.CurrentThread.CurrentUICulture;
        });

        await Application.Dispatcher.Invoke(async () =>
        {
            Thread.CurrentThread.CurrentCulture = current;
            Thread.CurrentThread.CurrentUICulture = uiCulture;
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

                                if (response is Task taskResponse)
                                {
                                    await taskResponse.ConfigureAwait(true);
                                    Type taskType = method.ReturnType;
                                    if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
                                    {
                                        response = taskType.GetProperty(nameof(Task<object>.Result))!.GetValue(taskResponse);
                                        reply.ValueType = taskType.GetGenericArguments()[0].AssemblyQualifiedName;
                                    }
                                    else
                                    {
                                        reply.ValueType = typeof(void).AssemblyQualifiedName;
                                    }
                                }
                                else
                                {
                                    reply.ValueType = method.ReturnType.AssemblyQualifiedName;
                                }

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

            current = Thread.CurrentThread.CurrentCulture;
            uiCulture = Thread.CurrentThread.CurrentUICulture;
        });

        Application.Dispatcher.Invoke(() =>
        {
            Thread.CurrentThread.CurrentCulture = current;
            Thread.CurrentThread.CurrentUICulture = uiCulture;
        });
        return reply;
    }


}
