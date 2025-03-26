using System.Runtime.InteropServices;

namespace OpenConnect.NC.SSO;

public static class DependenciesValidator
{
    public static bool AreDependenciesInstalled(){

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)){

            return AreBashDependenciesInstalled();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){

            return AreWindowsDependenciesInstalled();
        }

        return true;
    }


    private static bool AreBashDependenciesInstalled()
    {
        var openconnectIsInstalled = !string.IsNullOrEmpty(ShellHelper.Bash("which openconnect"));

        if (!openconnectIsInstalled){
            Console.WriteLine("Detected openconnect is not installed. Please install it before using this program.");
            return false;
        }

        return true;
    }
    private static bool AreWindowsDependenciesInstalled()
    {
        var openconnectIsInstalled = !string.IsNullOrEmpty(ShellHelper.Cmd("where openconnect.exe"));

        if (!openconnectIsInstalled){
            Console.WriteLine("Detected openconnect is not installed. Please install it before using this program.");
            return false;
        }
        return true;
    }
}
