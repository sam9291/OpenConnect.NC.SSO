using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using OpenConnect.NC.SSO;

var cancellationTask = GetConsoleCancelKeyPressTask();

IConfiguration configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();

var options = new CommandLineOptions();
configuration.Bind(options);

if(!options.IsValid())
{
    return;
}

if(!DependenciesValidator.AreDependenciesInstalled()){
    return;
}

Console.WriteLine("Starting openconnect nc sso");

var vpnServer = options.VpnServer;

if (!vpnServer.StartsWith("https://", StringComparison.OrdinalIgnoreCase)){
    vpnServer = "https://" + vpnServer;
}

using var playwright = await Playwright.CreateAsync();

Console.WriteLine($"Opening browser to connect to VPN Server {vpnServer}. Please login using your usual SSO process.");
var browser = await BrowserDetector.DetectAndLaunchBrowser(playwright, options);
var page = await browser.NewPageAsync();
await page.GotoAsync(vpnServer);
var context = browser.Contexts.Single();

Console.WriteLine("The browser will close when session is detected automatically or press [ctrl+c] in this terminal to cancel.");

const string SessionCookieName = "DSID";
BrowserContextCookiesResult? cookie = null;

while(!cancellationTask.IsCompleted && cookie is null)
{
    var cookies = await context.CookiesAsync();
    cookie = cookies.FirstOrDefault(x => x.Name == SessionCookieName);
    await Task.Delay(300);
}

if(cancellationTask.IsCompleted){
    return;
}

if (cookie is not null)
{
    Console.WriteLine("Detected VPN session cookie for authentication successfully. Starting VPN session...");
    Console.WriteLine("Press [ctrl + c] to stop VPN");
    await browser.CloseAsync();
    await browser.DisposeAsync();

    var additionalArguments = "";
    if (options.Script is not null){
        additionalArguments += $"--script {options.Script}";
    }

    if (options.AdditionalArguments is not null){
        additionalArguments += $" {options.AdditionalArguments}";
    }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
            ShellHelper.Bash($"sudo openconnect --protocol=nc --cookie=\"DSID={cookie.Value}\" {vpnServer} {additionalArguments}", true);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ShellHelper.Cmd($"openconnect.exe --protocol=nc --cookie=\"DSID={cookie.Value}\" {vpnServer} {additionalArguments}", true);
        }
        else
        {
            throw new NotSupportedException($"{RuntimeInformation.OSDescription} is not supported by this application");
        }

    await cancellationTask;
}
else
{
    Console.WriteLine("Failed to find session cookie. Please make sure you logged in through your SSO session successfully.");
    await browser.CloseAsync();
}

static Task GetConsoleCancelKeyPressTask()
{
    var taskCompletionSource = new TaskCompletionSource();

    Console.CancelKeyPress += (sender, eventArgs) =>
    {
        eventArgs.Cancel = true;

        Console.WriteLine("Cancellation requested. Stopping application...");
        taskCompletionSource.SetResult();
    };

    return taskCompletionSource.Task;
}
