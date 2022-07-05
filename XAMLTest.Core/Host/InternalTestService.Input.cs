using Grpc.Core;

namespace XamlTest.Host;

partial class InternalTestService
{
    public sealed override Task<KeyboardFocusResult> MoveKeyboardFocus(KeyboardFocusRequest request, ServerCallContext context)
        => MoveKeyboardFocus(request);
    protected abstract Task<KeyboardFocusResult> MoveKeyboardFocus(KeyboardFocusRequest request);

    public sealed override Task<InputResult> SendInput(InputRequest request, ServerCallContext context)
        => SendInput(request);
    protected abstract Task<InputResult> SendInput(InputRequest request);
}
