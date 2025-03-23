using System.Diagnostics;

namespace OpenConnect.NC.SSO;

public static class ShellHelper
{
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
}
