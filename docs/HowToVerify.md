# How to verify release binaries signatures

## Introduction

Downloading binaries on internet might be dangerous. When you download the binaries of the release of GRSPayServer Vault on our github release page, you only ensure that the uploader had access to our github repository.

This might be fine, but sometimes you download the same binaries from a different source, or you want additional assurance that those binaries are signed by the developers of the project. (In this case, Jackie)

If you do not care about who signed the executable and verifying the integrity of the files you downloaded, you don't have to read this document.

## Checking PGP signatures<a name="pgp"></a>

For this you need the `gpg` tool, make sure it is installed on your machine.

In the [release page](https://github.com/Groestlcoin/GRSPayServer.Vault/releases/latest), download:

1. The release binary of your choice.
2. The `SHA256SUMS.asc` file

Then we will go through how to install jackielove4u PGP keys on your system, and check the actual binaries.

### Importing Jackie's pgp keys (only first time)

This step should be done only one time. It makes sure your system knows Jackie's PGP Keys.

Jackie has a [keybase](https://keybase.io/jackielove4u) account that allow you to verify that his identity is linked to several well-known social media accounts.
And as you can see on his profile page, the PGP key `D11B D4F3 3F1D B499` is linked to his keybase identity.

You can import this key from keybase:

```bash
curl https://keybase.io/jackielove4u/pgp_keys.asc?fingerprint=287ae4ca1187c68c08b49cb2d11bd4f33f1db499 | gpg --import
```
or
```
keybase pgp pull jackielove4u
```

Alternatively, you can just download the file via the browser and run:

```bash
gpg --import pgp_keys.asc
```

This step won't have to be repeated the next time you need to check a signature.

### Checking the actual PGP signature

```
sha256sum --check SHA256SUMS.asc
```

You should see that the file you downloaded has the right hash:
```
GRSPayServerVault-1.0.7-setup.exe: OK
```

If you are on Windows you can check the hashes are identical manually:
```powershell
certUtil -hashfile GRSPayServerVault-1.0.7-setup.exe SHA256
type SHA256SUMS.asc
```

If you are on Mac:
```
shasum -a 256 --check SHA256SUMS.asc
```

You should see that the file you downloaded has the right hash:
```
GRSPayServerVault-osx-x64-1.0.7.dmg: OK
```

Then check the actual signature:

```
gpg --verify SHA256SUMS.asc
```

Which should output something like:

```
gpg: Signature made Sat Feb  5 20:40:47 2021 JST
gpg:                using RSA key D11BD4F33F1DB499
gpg: Good signature from "GRSPayServer Vault <jackielove4u@hotmail.com>" [unknown]
gpg: WARNING: This key is not certified with a trusted signature!
gpg:          There is no indication that the signature belongs to the owner.
Primary key fingerprint: 287A E4CA 1187 C68C 08B4 9CB2 D11B D4F3 3F1D B499
```
