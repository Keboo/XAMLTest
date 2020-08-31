using System;
using System.Diagnostics;

namespace XamlTest.Internal
{
    internal class ManagedApp : App
    {
        public ManagedApp(Process managedProcess, Protocol.ProtocolClient client, Action<string>? logMessage)
            : base(client, logMessage) 
            => ManagedProcess = managedProcess ?? throw new ArgumentNullException(nameof(managedProcess));

        public Process ManagedProcess { get; }

        public override void Dispose()
        {
            base.Dispose();
            ManagedProcess.Kill();
            ManagedProcess.WaitForExit();
        }
    }
}
