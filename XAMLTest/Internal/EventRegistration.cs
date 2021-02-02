using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XamlTest.Internal
{
    internal class EventRegistration : IEventRegistration
    {
        public Protocol.ProtocolClient Client { get; }
        public string EventId { get; }
        public string EventName { get; }
        public Serializer Serializer { get; }
        public Action<string>? LogMessage { get; }

        public EventRegistration(Protocol.ProtocolClient client,
            string eventId, string eventName,
            Serializer serializer, Action<string>? logMessage)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            LogMessage = logMessage;
        }

        public override string ToString() => $"{nameof(IEventRegistration)}: {EventName}";

        public async Task<IList<IEventInvocation>> GetInvocations()
        {
            EventInvocationsQuery eventInvocationQuery = new()
            {
                EventId = EventId
            };
            LogMessage?.Invoke($"{nameof(GetInvocations)}()");
            if (await Client.GetEventInvocationsAsync(eventInvocationQuery) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return reply.EventInvocations
                    .Select(x =>
                    {
                        return (IEventInvocation)new EventInvocation(x.Parameters.Cast<object>().ToArray());
                    })
                    .ToList();
            }
            throw new Exception("Failed to receive a reply");
        }

        public async ValueTask DisposeAsync()
        {
            EventUnregisterRequest eventInvocationQuery = new()
            {
                EventId = EventId
            };
            LogMessage?.Invoke($"{nameof(GetInvocations)}()");
            if (await Client.UnregisterForEventAsync(eventInvocationQuery) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return;
            }
            throw new Exception("Failed to receive a reply");
        }
    }

    internal class EventInvocation : IEventInvocation
    {
        public IReadOnlyList<object> Parameters { get; }

        public EventInvocation(IReadOnlyList<object> parameters)
        {
            Parameters = parameters;
        }
    }
}
