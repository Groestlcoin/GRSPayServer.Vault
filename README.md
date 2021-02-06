[![Build status](https://github.com/groestlcoin/GRSPayServer.Vault/workflows/CI/badge.svg)](https://github.com/groestlcoin/GRSPayServer.Vault/actions?query=workflow%3ACI)

# GRSPayServer.Vault

This project is composed of two parts:

* [BTCPayServer.Hwi](BTCPayServer.Hwi): An easy to use library wrapping the command line interface of the [hwi project](https://github.com/Groestlcoin/HWI).
* [BTCPayServer.Vault](BTCPayServer.Vault): A simple local web server providing access to the hardware wallet physically connected to your computer via hwi.

## Why GRSPayServer Vault

GRSPayServer Vault allows web applications to access your hardware wallet, this enables a better integrated user experience.

## How to install

### Direct download

The binaries are on our [release page](https://github.com/Groestlcoin/GRSPayServer.Vault/releases/latest).

### Via brew (Mac OS only)

You can use brew:

```bash
brew install grspayserver-vault
```

## How does GRSPayServer Vault work

When running the GRSPayServer Vault, a local webserver is hosted on `http://127.0.0.1:65092` which web applications, via your local browser, can connect to in order to interact with your hardware wallet.

The protocol is fairly simple:

First, the web application needs to make a permission request to the Vault by sending a HTTP request `GET` to `http://127.0.0.1:65092/hwi-bridge/v1/request-permission`

This will prompt the user to grant access to the web application and if the user accepts, the request returns HTTP 200. Note that internally, the Vault relies on the `ORIGIN` HTTP header to identify the web application requesting access.
If the access was granted previously, the request returns HTTP 200.

Second, the web application can query the hardware through `POST` requests to `http://127.0.0.1:65092/hwi-bridge/v1`.

```json
{
    "params": [ "param1", "param2" ]
}
````

Those parameters are then passed as-is to [hwi](https://github.com/Groestlcoin/HWI) and the result is returned as a string.

## Is it safe?

Hardware wallets have been created to protect your money, even if your computer was compromised.

However, while it protects your money, it will not protect your privacy if you allow an untrusted application to access your public keys.
This is why GRSPayServer Vault always ask permission to user first before allowing any web application to access your hardware wallet.

## How to build?

This is a two step process:

1. Install the latest version of the [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1)
2. Run `dotnet build`

If you want to run it for testing:

```bash
cd GRSPayServer.Vault
dotnet run
```

## Licence

This project is under MIT License.

## Special thanks

Fork of [BTCPayServerVault](https://github.com/btcpayserver/BTCPayServer.Vault)
Special thanks to [Wasabi Wallet](https://github.com/zkSNACKs/WalletWasabi), this code is based on their work, and as well to the bitcoin developers and [achow101](https://github.com/achow101) for the [HWI Project](https://github.com/bitcoin-core/HWI).
