using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
            KillProcess();
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            KillProcess();
        }

        private void KillProcess()
        {
            LogMessage?.Invoke("Killing process");
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            Process? process = Process.GetProcessById(ManagedProcess.Id);
            while (process?.HasExited == false && !cts.IsCancellationRequested)
            {
                process = Process.GetProcessById(ManagedProcess.Id);
            }
            LogMessage?.Invoke($"Process Exited? {process?.HasExited}");
            if (process?.HasExited == false)
            {
                LogMessage?.Invoke($"Invoking kill");
                process.Kill();
            }
            process?.WaitForExit();
        }
    }
}
