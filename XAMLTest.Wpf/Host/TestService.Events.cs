using Grpc.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using XamlTest.Event;

namespace XamlTest.Host;

partial class TestService
{
    protected override async Task<EventRegistrationResult> RegisterForEvent(EventRegistrationRequest request)
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

            if (element.GetType().GetEvent(request.EventName) is { } eventInfo)
            {
                EventRegistrar.Regsiter(reply.EventId, eventInfo, element);
            }
        });
        return reply;
    }
}
