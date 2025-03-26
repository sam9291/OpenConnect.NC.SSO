
namespace OpenConnect.NC.SSO;

public class CommandLineOptions
{
    public string VpnServer { get; set; } = "";
    public string? Script { get; set; }
    public string? AdditionalArguments { get; set; }
    public string? BrowserPath { get; set; }
    public BrowserTypeEnum? BrowserType { get; set; }

    internal bool IsValid()
    {
        if(string.IsNullOrEmpty(VpnServer)){
            Console.WriteLine("Please provide the VPN server to connect. Ex: OpenConnect.NC.SSO --VpnServer <vpn server url>");
            return false;
        }

        if(Script is not null && !File.Exists(Script)){
            Console.WriteLine("[--Script] Could not find provided script at location: " + Script);
            return false;
        }

        if(BrowserPath is not null){

            if(BrowserType is null){
                Console.WriteLine("[--BrowserType] Detected browser path provided, but missing --BrowserType argument. Supported browser types: [Chromium, Firefox]");
                return false;
            }

            if (!File.Exists(BrowserPath))
            {
                Console.WriteLine("[--BrowserPath] Could not find provided browser path at location: " + BrowserPath);
                return false;
            }
        }

        return true;
    }
}

public enum BrowserTypeEnum
{
    Chromium,
    Firefox
}
