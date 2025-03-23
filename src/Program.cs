using Microsoft.Playwright;
using OpenConnect.NC.SSO;

var cancellationTask = GetConsoleCancelKeyPressTask();

var openconnectIsInstalled = !string.IsNullOrEmpty(ShellHelper.Bash("which openconnect"));

if (!openconnectIsInstalled){
    Console.WriteLine("Detected openconnect is not installed. Please install it before using this program.");
    return;
}

Console.WriteLine("Starting openconnect nc sso");

if(args.Length != 1){
    Console.WriteLine("Please provide the VPN server to connect. Ex: openconnect.nc.sso <vpn server url>");
    return;
}

var vpnServer = args[0];

if (!vpnServer.StartsWith("https://", StringComparison.OrdinalIgnoreCase)){
    vpnServer = "https://" + vpnServer;
}

Console.WriteLine("Installing playwright dependencies");

var exitCode = Microsoft.Playwright.Program.Main(["install-deps"]);

if (exitCode != 0)
{
    Console.WriteLine("Failed to install dependencies");
    Environment.Exit(exitCode);
}

using var playwright = await Playwright.CreateAsync();

Console.WriteLine($"Opening browser to connect to VPN Server {vpnServer}. Please login using your usual SSO process.");
var browser = await BrowserDetector.DetectAndLaunchBrowser(playwright);
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
    ShellHelper.Bash($"sudo openconnect --protocol=nc --cookie=\"DSID={cookie.Value}\" {vpnServer}", true);
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
