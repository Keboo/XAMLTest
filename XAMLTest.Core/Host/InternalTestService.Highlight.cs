using Grpc.Core;

namespace XamlTest.Host;

partial class InternalTestService
{
    public sealed override Task<HighlightResult> HighlightElement(HighlightRequest request, ServerCallContext context)
        => HighlightElement(request);
    protected abstract Task<HighlightResult> HighlightElement(HighlightRequest request);
}
