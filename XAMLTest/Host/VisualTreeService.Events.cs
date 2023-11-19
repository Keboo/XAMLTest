using Grpc.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XamlTest.Event;

namespace XamlTest.Host;

partial class VisualTreeService
{
    public override async Task<EventRegistrationResult> RegisterForEvent(EventRegistrationRequest request, ServerCallContext context)
    {
        EventRegistrationResult reply = new()
        {
            EventId = Guid.NewGuid().ToString()
        };
        await Application.Dispatcher.InvokeAsync(() =>
        {
            DependencyObject? element = GetCachedElement<DependencyObject>(request.ElementId);
            if (element is null)
            {
                reply.ErrorMessages.Add("Could not find element");
                return;
            }

            Type elementType = element.GetType();
            if (elementType.GetEvent(request.EventName) is { } eventInfo)
            {
                EventRegistrar.Regsiter(reply.EventId, eventInfo, element);
            }
            else
            {
                reply.ErrorMessages.Add($"Could not find event '{request.EventName}' on {elementType.FullName}");
            }
        });
        return reply;
    }

    public override Task<EventUnregisterResult> UnregisterForEvent(EventUnregisterRequest request, ServerCallContext context)
    {
        EventUnregisterResult reply = new();
        if (!EventRegistrar.Unregister(request.EventId))
        {
            reply.ErrorMessages.Add("Failed to unregister event");
        }
        return Task.FromResult(reply);
    }

    public override Task<EventInvocationsResult> GetEventInvocations(EventInvocationsQuery request, ServerCallContext context)
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
