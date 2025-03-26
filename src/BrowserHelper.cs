using Microsoft.Playwright;

namespace OpenConnect.NC.SSO;

public static class BrowserDetector
{
    public static async Task<IBrowser> DetectAndLaunchBrowser(IPlaywright playwright, CommandLineOptions options)
    {
        if (options.BrowserPath is not null && options.BrowserType is not null){
            Console.WriteLine($"Using provided browser path [{options.BrowserPath}]");
            return options.BrowserType switch
            {
                BrowserTypeEnum.Chromium => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    ExecutablePath = options.BrowserPath,
                    Headless = false,
                }),
                BrowserTypeEnum.Firefox => await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    ExecutablePath = options.BrowserPath,
                    Headless = false,
                }),
                _ => throw new NotSupportedException($"Browser type {options.BrowserType} not supported."),
            };
        }
        var browserType = playwright.Chromium;

        var browserPath = ShellHelper.Bash("which microsoft-edge");

        if (string.IsNullOrEmpty(browserPath)){
            browserPath = ShellHelper.Bash("which chrome");
        }
        if (string.IsNullOrEmpty(browserPath)){
            browserPath = ShellHelper.Bash("which google-chrome-stable");
        }
        if (string.IsNullOrEmpty(browserPath)){
            browserPath = ShellHelper.Bash("which chromium");
        }
        if (string.IsNullOrEmpty(browserPath)){
            browserPath = ShellHelper.Bash("which firefox");
            browserType = playwright.Firefox;
        }

        if (string.IsNullOrEmpty(browserPath)){
            throw new Exception("Unable to find installed browser. Supported are: [microsoft-edge, chrome, chromium and firefox]");
        }

        browserPath = browserPath.Trim().Trim('\r').Trim('\n');

        Console.WriteLine($"Detected browser path [{browserPath}]");
        var browser = await browserType.LaunchAsync(new BrowserTypeLaunchOptions{
            ExecutablePath = browserPath,
            Headless = false,
        });

        return browser;
    }
}
