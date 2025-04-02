# openconnect nc sso

This small program is to help out connecting to Juniper Network Connect (nc) using SSO through browser login. The implementation uses Playwright to launch a browser allowing you to go through your SSO authentication flow and to extract the authentication cookie token used with openconnect to start the VPN connection.

When running the application, if you have all dependencies installed, a browser will open with the provided VPN server url. To connect to the VPN, use the following steps:

1. Complete your SSO login workflow and keep the browser open
2. The browser will be closed automatically when the session cookie has been detected and the VPN connection will start using openconnect
3. To stop the VPN connection, press [ctrl + c]

## Dependencies

Before running this application, you will need to install the openconnect CLI and a chromium based or firefox browser.

```shell
sudo apt-get install network-manager-openconnect 
```

### On Windows

You can install [gui.openconnect-vpn.net](https://gui.openconnect-vpn.net/) which includes openconnect.exe for windows. Make sure to add the installed directory to your path so you can run openconnect.exe in the CMD.

### On macOS

You can install openconnect using homebrew:

```shell

brew install openconnect
```

## Getting started

If you clone this repository and have the dotnet cli installed, you can simply run in the src directory:

```shell
dotnet run --VpnServer <VPN url to connect>
```

If you use the published executable.

```shell
OpenConnect.NC.SSO --VpnServer <VPN url to connect>
```

Once connected, leave your terminal session open to keep the VPN open or press [ctrl + c] to stop the connection.

## Advanced options

The application supports the following arguments:

- `--VpnServer`: URL of the VPN server to connect to
- `--BrowserPath`: [Optional] path of the browser to use. When not provided, this app will attempt to look for the following browsers in order: `microsoft-edge`, `google-chrome-stable`, `chrome`, `chromium`, `firefox`
- `--BrowserType`: [Required when specifying BrowserPath] `Chromium` or `Firefox`
- `--Script`: [Optional] openconnect script argument
- `--AdditionalArguments`: [Optional] any additional openconnect arguments to append when connecting
