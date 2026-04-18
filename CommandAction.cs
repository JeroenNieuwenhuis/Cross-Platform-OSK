using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Typo;

[JsonObject(MemberSerialization.OptIn)]
public sealed class CommandAction : IAction
{
    [JsonProperty]
    public string? startCommand { get; set; }

    [JsonProperty]
    public string? stopCommand { get; set; }

    [JsonProperty]
    public string? workingDirectory { get; set; }

    public void Start()
    {
        _ = RunCommandAsync(startCommand);
    }

    public void Stop()
    {
        _ = RunCommandAsync(stopCommand);
    }

    private async Task RunCommandAsync(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        try
        {
            using Process? process = Process.Start(CreateStartInfo(command));
            if (process == null)
            {
                return;
            }

            await process.WaitForExitAsync().ConfigureAwait(false);
        }
        catch
        {
            // Swallow command failures so UI input handling stays responsive.
        }
    }

    private ProcessStartInfo CreateStartInfo(string command)
    {
        ProcessStartInfo startInfo;

#if _WINDOWS
        startInfo = new ProcessStartInfo("cmd.exe", $"/C {command}");
#else
        startInfo = new ProcessStartInfo("/bin/bash", $"-lc \"{EscapeForBash(command)}\"");
#endif

        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;

        if (!string.IsNullOrWhiteSpace(workingDirectory))
        {
            startInfo.WorkingDirectory = workingDirectory;
        }

        return startInfo;
    }

#if !_WINDOWS
    private static string EscapeForBash(string command)
    {
        return command.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
#endif
}
