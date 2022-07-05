using Grpc.Core;

namespace XamlTest.Host;

partial class InternalTestService
{
    public sealed override Task<RemoteInvocationResult> RemoteInvocation(RemoteInvocationRequest request, ServerCallContext context)
        => RemoteInvocation(request);
    protected abstract Task<RemoteInvocationResult> RemoteInvocation(RemoteInvocationRequest request);
}
