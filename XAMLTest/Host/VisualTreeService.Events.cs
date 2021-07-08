using Grpc.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using XamlTest.Event;

namespace XamlTest.Host
{
    partial class VisualTreeService
    {
        public override async Task<EventRegistrationResponse> RegisterForEvent(EventRegistrationRequest request, ServerCallContext context)
        {
            EventRegistrationResponse reply = new()
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

                if (element.GetType().GetEvent(request.EventName) is { } eventInfo)
                {
                    EventRegistrar.Regsiter(reply.EventId, eventInfo, element);
                }
            });
            return reply;
        }

        public override Task<EventUnregisterResponse> UnregisterForEvent(EventUnregisterRequest request, ServerCallContext context)
        {
            EventUnregisterResponse reply = new();
            if (!EventRegistrar.Unregister(request.EventId))
            {
                reply.ErrorMessages.Add("Failed to unregister event");
            }
            return Task.FromResult(reply);
        }

        public override Task<EventInvocationsResponse> GetEventInvocations(EventInvocationsQuery request, ServerCallContext context)
        {
            EventInvocationsResponse reply = new()
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
}
