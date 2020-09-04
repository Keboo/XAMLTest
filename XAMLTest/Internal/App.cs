using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XamlTest.Internal
{

    internal class App : IApp
    {
        public App(Protocol.ProtocolClient client, Action<string>? logMessage)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            LogMessage = logMessage;
        }

        protected Protocol.ProtocolClient Client { get; }
        protected Action<string>? LogMessage { get; }

        public virtual void Dispose()
        {
            var request = new ShutdownRequest
            {
                ExitCode = 0
            };
            if (Client.Shutdown(request) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }

                return;
            }
            throw new Exception("Failed to get a reply");
        }

        public virtual async ValueTask DisposeAsync()
        {
            var request = new ShutdownRequest
            {
                ExitCode = 0
            };
            if (await Client.ShutdownAsync(request) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return;
            }
            throw new Exception("Failed to get a reply");
        }

        public async Task Initialize(string applicationResourceXaml, params string[] assemblies)
        {
            var request = new ApplicationConfiguration
            {
                ApplicationResourceXaml = applicationResourceXaml
            };
            request.AssembliesToLoad.AddRange(assemblies);
            if (await Client.InitializeApplicationAsync(request) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return;
            }
            throw new Exception("Failed to get a reply");
        }

        public async Task<IWindow> CreateWindow(string windowXaml)
        {
            var request = new WindowConfiguration()
            {
                Xaml = windowXaml,
                FitToScreen = true
            };
            if (await Client.CreateWindowAsync(request) is { } reply)
            {
                if (LogMessage is { })
                {
                    foreach(string logsMessage in reply.LogMessages)
                    {
                        LogMessage(logsMessage);
                    }
                }
                if (reply.ErrorMessages.Any())
                {
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return new Window(Client, reply.WindowsId);
            }
            throw new Exception("Failed to get a reply");
        }

        public async Task<IWindow?> GetMainWindow()
        {
            if (await Client.GetMainWindowAsync(new GetWindowsQuery()) is { } reply &&
                reply.WindowIds.Count == 1)
            {
                return new Window(Client, reply.WindowIds[0]);
            }
            return null;
        }

        public async Task<IResource> GetResource(string key)
        {
            var query = new ResourceQuery
            {
                Key = key
            };

            if (await Client.GetResourceAsync(query) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                if (!string.IsNullOrWhiteSpace(reply.ValueType))
                {
                    return new Resource(reply.Key, reply.ValueType, reply.Value);
                }
                throw new Exception($"Resource with key '{reply.Key}' not found");
            }

            throw new Exception("Failed to receive a reply");
        }

        public async Task<IReadOnlyList<IWindow>> GetWindows()
        {
            if (await Client.GetWindowsAsync(new GetWindowsQuery()) is { } reply)
            {
                return reply.WindowIds.Select(x => new Window(Client, x)).ToList();
            }
            return Array.Empty<IWindow>();
        }

    }
}
