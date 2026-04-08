# FastTelecom

A self-service desktop portal for Syrian Telecom. View your account, browse available internet bundles, monitor active subscriptions, and purchase new bundles all using a desktop app with automatic updates.

Built with the help of:

![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Avalonia](https://img.shields.io/badge/Avalonia_UI-8B44AC?style=for-the-badge&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-2088FF?style=for-the-badge&logo=github-actions&logoColor=white)
![xUnit](https://img.shields.io/badge/xUnit-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Velopack](https://img.shields.io/badge/Velopack-00B4D8?style=for-the-badge&logoColor=white)
![Nuke](https://img.shields.io/badge/Nuke_Build-1E1E2E?style=for-the-badge&logoColor=white)

--- 

## Why FastTelecom?

The official Syrian Telecom self-service portal is slow, unreliable, and has a dated interface. Pages take a long time to load, the UI is cluttered, and requests frequently fail without clear feedback.

FastTelecom was built to fix that:

- **Faster experience**: a desktop app with no browser overhead
- **Cleaner UI**: modern design instead of the buggy and awful User Interface.
- **Reliable connections**: built-in retry logic that automatically reattempts failed requests up to 5 times, so you don't have to keep refreshing
- **Automatic updates**: the app keeps itself up to date without you doing anything
- **I WILL REMEMBER YOU!**: The app will remember you with a tick of a button.

---

## Architecture

The project follows a **Clean Architecture**(Layered/Onion) pattern with four layers, each as its own .NET project:

```
FastTelecom.Domain            (Core interfaces, models, zero dependencies)
    |
FastTelecom.Application       (Use cases services, DTOs, session management)
    |
FastTelecom.Infrastructure    (API clients HTTP, encryption, external services)
    |
FastTelecom.AvaloniaUI        (Presentation MVVM views, navigation, updates)
```

**Why this structure?**

- **Domain**: defines what the app needs (interfaces like `ITarasClient`, `IBundleClient`) without knowing how it's done
- **Application**: contains the business logic, validation, mapping, error handling and depends only on Domain
- **Infrastructure**: implements the actual API calls, encryption, and HTTP configuration
- **AvaloniaUI**: is the desktop frontend, consuming Application services via dependency injection

Each layer only references the one below it. The UI never talks to the API directly. 

---

## How the API Works

FastTelecom interacts with the Syrian Telecom self-service portal with the help of reverse engineered API endpoints. All communication uses HTTPS POST requests with specific encoded parameters.

### Authentication & Encryption

Every request to the Taras portal uses a dual-credential system:

1. **Decoy credentials**: MD5-hashed placeholder strings sent as `userName` and `userPswd` fields
2. **Real credentials**: RSA-encrypted (PKCS#1, 2048-bit public key) username and password, hex-encoded, sent as `uNC` and `uPC` fields

The server validates the encrypted credentials while the decoy values satisfy the form structure. A shared `CookieContainer` maintains session state across all requests.

All requests include browser-like headers (`User-Agent`, `X-Requested-With: XMLHttpRequest`) to match what the web portal sends.

### The Three Gates

The API is organized around three main operations, each identified by an `F_ID` parameter:

#### Gate 1: Login (`F_ID=1`)

```
POST https://www.syriantelecom.com.sy/include/TarasSelfPortal.php
```

Authenticates the subscriber and returns account information (`SubscriberInfo`), including subscriber ID, IP address, account status, session limits, and expiry dates.

The response can be a JSON array (first element is taken), a single JSON object, or the string `"NOTOK"` for invalid credentials.

#### Gate 3: Active Bundles (`F_ID=3`)

```
POST https://www.syriantelecom.com.sy/include/TarasSelfPortal.php
```

Same endpoint as login, different `F_ID`. Returns an array of `ActiveBundle` objects with usage data: product name, max volume, free volume remaining, speed tier, session count, effective/expiry dates, and monthly accumulated usage.

#### Gate 6: Available Bundles (`F_ID=6`)

```
POST https://www.syriantelecom.com.sy/php/Con_Flex1a225_5_CCBS2.php
```

Different endpoint. Returns a `BundlesApiResponse` containing a `Basic` account identifier and an array of purchasable bundles with ID, name, price, volume, and availability status.

### Purchasing a Bundle

```
POST https://www.syriantelecom.com.sy//Sync/abtw225_5_send.php
```

Purchase requests use a different format — the payload includes a JSON-serialized `as_request` field containing the username, basic account ID, and an array of product IDs. The server responds with a status code (`200` = success) and an optional message.

The Application layer checks `Code == 200` and verifies no error message is present before confirming the purchase to the user.

---

## Tech Stack

| Component | Technology |
|---|---|
| **Framework** | .NET 10 |
| **UI** | [Avalonia](https://avaloniaui.net/) 11.3 with Fluent theme |
| **Pattern** | MVVM via [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) |
| **DI** | Microsoft.Extensions.DependencyInjection |
| **HTTP** | HttpClient with typed clients and shared cookie container |
| **Encryption** | RSA (PKCS#1) + MD5 hashing via custom `CryptoService` |
| **Versioning** | [MinVer](https://github.com/adamralph/minver) (git tag-based) |
| **Auto-update** | [Velopack](https://velopack.io/) with GitHub Releases |
| **Build** | [Nuke](https://nuke.build/) build automation |
| **CI/CD** | GitHub Actions |
| **Testing** | xUnit + NSubstitute |

---

## CI/CD Pipeline

The project has two automated workflows:

### CI — Every Push

```
Push to any branch
    -> Ubuntu runner
    -> Restore -> Compile -> Test
```

Runs all unit tests on every push to verify the build is healthy. If tests fail, the push is flagged.

### Release — On Tag Push

```
Push a tag (v*)
    -> Windows runner
    -> Restore -> Compile -> Test -> Publish -> Velopack Pack
    -> Create GitHub Release with all assets
```

When you push a version tag:

```bash
git tag v1.2.0
git push origin v1.2.0
```

The Release workflow:

1. Checks out the repo with full history (so MinVer reads the tag)
2. Installs the `vpk` CLI tool
3. Runs `Nuke Pack` which builds, tests, publishes (self-contained, win-x64), and packages with Velopack
4. Creates a GitHub Release with auto-generated release notes and uploads all assets

### Release Assets

Each release contains:

| File | Purpose |
|---|---|
| `FastTelecom-win-Setup.exe` | Installer with automatic updates |
| `FastTelecom-win-Portable.zip` | Portable version, no install needed |
| `releases.win.json` | Update manifest (used by auto-updater) |
| `FastTelecom-{version}-full.nupkg` | Full update package |
| `RELEASES` | Legacy compatibility file |

---

## Auto-Updates

The app uses Velopack with GitHub Releases as the update source.

**How it works:**

1. After login, the app silently checks `releases.win.json` from the latest GitHub Release
2. If a newer version exists, a centered modal prompt appears asking the user to update
3. If the user clicks **"Update now"** the update downloads, installs, and the app restarts automatically
4. If the user clicks **"Not now"** a green **"Update available"** button appears in the sidebar for later

> [!WARNING]
> Auto-updates only work for installs via the Velopack Setup.exe. Portable users need to download new versions manually.

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Nuke](https://nuke.build/) (optional, for build automation)

### Clone and Run

```bash
git clone https://github.com/LolghElmo/FastTelecom.git
cd FastTelecom
dotnet run --project FastTelecom.AvaloniaUI
```

### Build with Nuke

```bash
# Run tests
./build.cmd Test

# Publish and package a release
./build.cmd Pack --version 1.0.0
```

### Run Tests

```bash
dotnet test
```

---

## Project Structure

```
FastTelecom/
  FastTelecom.Core/                  # Domain layer
    Interfaces/
      ITarasClient.cs                # Login + active bundles contract
      IBundleClient.cs               # Available bundles + purchase contract
      ICryptoService.cs              # Encryption contract
    Models/
      LoginResponse.cs
      SubscriberInfo.cs
      Bundle.cs, BundlesApiResponse.cs
      ActiveBundle.cs, ActiveBundleAccumulateInfo.cs
      PurchaseApiResponse.cs, PurchaseItemResult.cs

  FastTelecom.Application/           # Application layer
    Services/
      AuthenticationService.cs       # Login logic + session management
      BundleService.cs              # Bundle CRUD + data mapping
    DTOs/                           # Data transfer objects for the UI
    SessionStore.cs                 # In-memory session state

  FastTelecom.Infrastructure/        # Infrastructure layer
    Services/
      TarasClient.cs                # HTTP client for Taras portal
      BundleClient.cs               # HTTP client for bundle endpoints
      CryptoService.cs              # RSA encryption + MD5 hashing
    DependencyInjection.cs

  FastTelecom.AvaloniaUI/           # Presentation layer
    Views/                          # AXAML views
    ViewModels/                     # MVVM view models
    Services/
      NavigationService.cs          # Page navigation
      CredentialStore.cs            # AES-encrypted credential persistence
    Converters/

  FastTelecom.Application.Tests/    # Unit tests
  FastTelecom.Domain.Tests/

  build/
    Build.cs                        # Nuke build targets
  .github/workflows/
    CI.yml                          # Test on every push
    Release.yml                     # Package + release on tag push
```

## License

This project is for educational and personal use. It interacts with Syrian Telecom's public facing web portal and is not affiliated with or endorsed by Syrian Telecom.
