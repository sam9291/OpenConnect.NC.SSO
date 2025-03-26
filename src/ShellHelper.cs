using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenConnect.NC.SSO;

public static class ShellHelper
{
    public static string RunShellCommand(this string cmd, bool streamOutputToConsole = false)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)){

            return Bash(cmd, streamOutputToConsole);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){

            return Cmd(cmd, streamOutputToConsole);
        }

        throw new NotSupportedException($"{RuntimeInformation.OSDescription} is not supported by this application");
    }

    public static string Bash(this string cmd, bool streamOutputToConsole = false)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");

        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        string result = "";
        if (streamOutputToConsole){
            while (!process.StandardOutput.EndOfStream) {
                string? line = process.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }
        }
        else
        {
            result = process.StandardOutput.ReadToEnd();
        }
        process.WaitForExit();

        return result;
    }

    public static string Cmd(this string cmd, bool streamOutputToConsole = false)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");

        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        string result = "";
        if (streamOutputToConsole){
            while (!process.StandardOutput.EndOfStream) {
                string? line = process.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }
        }
        else
        {
            result = process.StandardOutput.ReadToEnd();
        }
        process.WaitForExit();

        return result;
    }
}
