using Grpc.Core;
using XamlTest.Event;

namespace XamlTest.Host;

partial class InternalTestService
{
    public override Task<EventRegistrationResult> RegisterForEvent(EventRegistrationRequest request, ServerCallContext context)
        => RegisterForEvent(request);
    protected abstract Task<EventRegistrationResult> RegisterForEvent(EventRegistrationRequest request);

    public override Task<EventUnregisterResult> UnregisterForEvent(EventUnregisterRequest request, ServerCallContext context)
    {
        EventUnregisterResult reply = new();
        if (!EventRegistrar.Unregister(request.EventId))
        {
            reply.ErrorMessages.Add("Failed to unregister event");
        }
        return Task.FromResult(reply);
    }

    public sealed override Task<EventInvocationsResult> GetEventInvocations(EventInvocationsQuery request, ServerCallContext context)
    {
        EventInvocationsResult reply = new()
        {
            EventId = request.EventId,
        };
        var invocations = EventRegistrar.GetInvocations(request.EventId);
        if (invocations is null)
        {
            reply.ErrorMessages.Add("Event was not registered");
        }
        else
        {
            reply.EventInvocations.AddRange(
                invocations.Select(
                    array =>
                    {
                        EventInvocation rv = new();
                        rv.Parameters.AddRange(array.Select(item => GetItemString(item)));
                        return rv;
                    }));
        }


        return Task.FromResult(reply);

        string GetItemString(object item)
        {
            return Serializer.Serialize(item.GetType(), item)
                ?? item?.ToString()
                ?? item?.GetType().FullName
                ?? "<null>";
        }
    }
}
