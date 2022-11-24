using EnvDTE;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DTEProcess = EnvDTE.Process;
using Process = System.Diagnostics.Process;

namespace XamlTest;

public class VisualStudioAttacher
{
    [DllImport("ole32.dll")]
    private static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

    [DllImport("ole32.dll")]
    private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

    public static string? GetSolutionForVisualStudio(Process visualStudioProcess)
    {
        if (TryGetVsInstance(visualStudioProcess.Id, out _DTE? visualStudioInstance))
        {
            try
            {
                return visualStudioInstance.Solution.FullName;
            }
            catch (Exception)
            {
            }
        }
        return null;
    }

    public static async Task AttachVisualStudioToProcess(Process applicationProcess)
    {
        await Wait.For(() => Task.FromResult(AttachVisualStudioToProcessImplementation(applicationProcess)),
            retry: new Retry(5, TimeSpan.FromSeconds(15)),
            message: "Failed to attach Visual Studio to the XAMLTest host process");

        static bool AttachVisualStudioToProcessImplementation(Process applicationProcess)
        {
            if (GetAttachedVisualStudio(Process.GetCurrentProcess()) is { } vsProcess)
            {
                DTEProcess? processToAttachTo = vsProcess
                    .Parent
                    .LocalProcesses
                    .Cast<DTEProcess>()
                    .FirstOrDefault(process => process.ProcessID == applicationProcess.Id);

                if (processToAttachTo != null)
                {
                    processToAttachTo.Attach();
                    return true;
                }
            }
            return false;
        }
    }

    private static DTEProcess? GetAttachedVisualStudio(Process applicationProcess)
    {
        IEnumerable<Process> visualStudios = GetVisualStudioProcesses();

        foreach (Process visualStudio in visualStudios)
        {
            if (TryGetVsInstance(visualStudio.Id, out _DTE? visualStudioInstance))
            {
                try
                {
                    foreach (DTEProcess? debuggedProcess in visualStudioInstance.Debugger.DebuggedProcesses)
                    {
                        if (debuggedProcess?.ProcessID == applicationProcess.Id)
                        {
                            return debuggedProcess;
                        }
                    }
                }
                catch (Exception)
                { }
            }
        }
        return null;
    }

    private static IEnumerable<Process> GetVisualStudioProcesses()
        => Process.GetProcesses().Where(o => o.ProcessName.Contains("devenv"));

    private static bool TryGetVsInstance(int processId, [NotNullWhen(true)] out _DTE? instance)
    {
        IntPtr numFetched = IntPtr.Zero;
        IRunningObjectTable runningObjectTable;
        IEnumMoniker monikerEnumerator;
        IMoniker[] monikers = new IMoniker[1];

        GetRunningObjectTable(0, out runningObjectTable);
        runningObjectTable.EnumRunning(out monikerEnumerator);
        monikerEnumerator.Reset();

        while (monikerEnumerator.Next(1, monikers, numFetched) == 0)
        {
            CreateBindCtx(0, out IBindCtx ctx);

            monikers[0].GetDisplayName(ctx, null, out string runningObjectName);

            runningObjectTable.GetObject(monikers[0], out object runningObjectVal);

            if (runningObjectVal is _DTE dte && runningObjectName.StartsWith("!VisualStudio") &&
                runningObjectName.Split(':') is { } parts && 
                parts.Length >= 2 &&
                int.TryParse(parts[1], out int currentProcessId) &&
                currentProcessId == processId)
            {
                instance = dte;
                return true;
            }
        }

        instance = null;
        return false;
    }
}
