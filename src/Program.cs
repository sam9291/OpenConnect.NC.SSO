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

var openconnectIsInstalled = !string.IsNullOrEmpty(ShellHelper.Bash("which openconnect"));

if (!openconnectIsInstalled){
    Console.WriteLine("Detected openconnect is not installed. Please install it before using this program.");
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

Console.WriteLine("When SSO login is complete in the browser, press [enter] in this terminal to connect using openconnect.");
while(Console.ReadKey().Key != ConsoleKey.Enter){
}

if(cancellationTask.IsCompleted){
    return;
}

const string SessionCookieName = "DSID";
var cookies = await context.CookiesAsync();
var cookie = cookies.FirstOrDefault(x => x.Name == SessionCookieName);

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

    ShellHelper.Bash($"sudo openconnect --protocol=nc --cookie=\"DSID={cookie.Value}\" {vpnServer} {additionalArguments}", true);
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
