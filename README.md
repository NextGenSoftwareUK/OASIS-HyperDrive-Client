# OASIS HyperDrive Client

A cross-platform system-tray desktop application that gives users a native file-explorer experience over the [OASIS HyperDrive](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-Or-Avatars) — the decentralised, multi-provider storage layer built into the OASIS Architecture.

Like OneDrive, Google Drive, or Dropbox, the client lives in the system tray and lights up when something needs attention. Double-clicking it opens a purpose-built file browser showing the user's **holons, files, NFTs, GeoNFTs**, and other OASIS digital assets stored across all enabled providers.

---

## Features

- **System tray icon** — neon-glowing "O" rendered via SkiaSharp, colour-coded by health state (cyan = healthy, yellow = degraded, red = error, grey = offline, etc.)
- **File browser** — DataGrid view of all holons/files/NFTs/GeoNFTs with sidebar content-type navigation, provider filter, and search
- **Full CRUD** — upload, download, rename, delete (soft or hard), view metadata
- **Send to Avatar** — search for another avatar and transfer any holon/file to their HyperDrive
- **Right-click context menu** — all operations accessible without the toolbar
- **HyperDrive dashboard** — live provider health, uptime, latency, active alerts, AI recommendations
- **Settings** — ONODE API URL, default provider, auto-start on login, dashboard refresh rate, per-event notification toggles
- **OS notifications** — toast alerts for failover, provider down, replication complete, file received, upload complete
- **Auto-start on login** — Windows registry, macOS LaunchAgent, Linux `.desktop` autostart
- **Cross-platform** — Windows, macOS (x64 + Apple Silicon), Linux

---

## Technology Stack

| Layer | Technology |
|---|---|
| UI Framework | Avalonia UI 12.1.0 |
| Language | C# 12, .NET 10 |
| MVVM | ReactiveUI 23.x |
| HTTP / API | `HttpClient` + `System.Text.Json` + Polly retry |
| Tray icon | `Avalonia.Controls.TrayIcon` (built-in) |
| Icon rendering | SkiaSharp (neon-O with per-state glow) |
| Notifications | `Avalonia.Controls.Notifications` (`WindowNotificationManager`) |
| Logging | Serilog (file + console sinks) |
| DI Container | `Microsoft.Extensions.DependencyInjection` |
| Background polling | `Microsoft.Extensions.Hosting` (`BackgroundService`) |
| Session storage | File-based (`%APPDATA%/OasisHyperDriveClient/.session`, base64 JWT) |
| Build / packaging | `dotnet publish` self-contained single-file |

---

## Solution Structure

```
OasisHyperDriveClient/
├── src/
│   ├── OasisHyperDriveClient.Core/          # Shared library — models, API, auth, services
│   │   ├── Api/
│   │   │   ├── OasisApiClient.cs            # HttpClient wrapper, JWT injection, OASISResult<T>
│   │   │   ├── DataService.cs               # api/data/* endpoints
│   │   │   ├── HyperDriveService.cs         # api/hyperDrive/* endpoints
│   │   │   └── AvatarService.cs             # api/avatar/* endpoints
│   │   ├── Auth/
│   │   │   ├── AuthService.cs               # Login, session restore, logout
│   │   │   ├── ICredentialStore.cs
│   │   │   └── FileCredentialStore.cs       # Base64 JWT in %APPDATA%
│   │   ├── Models/
│   │   │   ├── Holon.cs                     # Holon + HolonViewModel
│   │   │   ├── Avatar.cs                    # AvatarInfo, AuthenticateRequest/Response
│   │   │   ├── TrayState.cs                 # TrayState enum + TrayStateInfo
│   │   │   ├── HyperDriveDashboard.cs       # Dashboard, metrics, alerts, config
│   │   │   └── OASISResult.cs               # Generic API result wrapper
│   │   └── Services/
│   │       ├── AppSettings.cs               # Settings model, Load()/Save()
│   │       ├── HyperDriveMonitorService.cs  # BackgroundService — polls dashboard every N sec
│   │       ├── IAutoStartService.cs
│   │       └── INotificationService.cs
│   │
│   └── OasisHyperDriveClient/               # Avalonia UI project
│       ├── App.axaml / App.axaml.cs         # DI wiring, tray setup, startup flow
│       ├── Program.cs
│       ├── Assets/
│       │   └── Styles.axaml                 # Neon colour palette + component styles
│       ├── Services/
│       │   ├── TrayIconRenderer.cs          # SkiaSharp neon-O PNG renderer
│       │   ├── AvaloniaNotificationService.cs
│       │   ├── WindowsAutoStartService.cs
│       │   ├── LinuxAutoStartService.cs
│       │   └── MacAutoStartService.cs
│       ├── ViewModels/
│       │   ├── TrayIconViewModel.cs
│       │   ├── FileBrowserViewModel.cs
│       │   ├── LoginViewModel.cs
│       │   ├── MetadataViewModel.cs
│       │   ├── DashboardViewModel.cs
│       │   ├── SettingsViewModel.cs
│       │   ├── RenameViewModel.cs
│       │   ├── DeleteConfirmViewModel.cs
│       │   └── SendToAvatarViewModel.cs
│       └── Views/
│           ├── FileBrowserWindow.axaml(.cs)
│           ├── LoginWindow.axaml(.cs)
│           ├── MetadataWindow.axaml(.cs)
│           ├── DashboardWindow.axaml(.cs)
│           ├── SettingsWindow.axaml(.cs)
│           ├── RenameDialog.axaml(.cs)
│           ├── DeleteConfirmDialog.axaml(.cs)
│           └── SendToAvatarDialog.axaml(.cs)
│
└── tests/
    └── OasisHyperDriveClient.Tests/
        ├── HyperDriveMonitorServiceTests.cs
        ├── HolonViewModelTests.cs
        └── AppSettingsTests.cs
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A running OASIS ONODE instance (or access to `https://api.oasis.ac`)
- An OASIS avatar account

---

## Getting Started

### Clone and Build

```bash
git clone https://github.com/NextGenSoftwareUK/OasisHyperDriveClient.git
cd OasisHyperDriveClient
dotnet build
```

### Run (Debug)

```bash
dotnet run --project src/OasisHyperDriveClient/OasisHyperDriveClient.csproj
```

The app starts in the system tray. Double-click the tray icon to open the file browser.

### Configure API URL

By default the client connects to `https://api.oasis.ac`. Override for local development:

```bash
# Environment variable
OASIS_API_URL=http://localhost:5000 dotnet run ...

# Or via Settings window after login
```

### Run Tests

```bash
dotnet test tests/OasisHyperDriveClient.Tests/
```

---

## Building for Distribution

Self-contained single-file binaries:

**Windows (x64)**
```powershell
.\build-win.ps1 -Version 1.0.0
# Output: dist\win\OasisHyperDriveClient.exe
```

**Linux (x64)**
```bash
bash build-linux.sh 1.0.0
# Output: dist/linux/OasisHyperDriveClient
```

**macOS**
```bash
bash build-mac.sh 1.0.0
# Output: dist/mac-x64/ and dist/mac-arm64/
```

---

## Tray Icon States

| State | Colour | Meaning |
|---|---|---|
| Disabled | Grey `#808080` | Offline / ONODE unreachable |
| Connecting | Blue `#4488FF` | Authenticating or reconnecting |
| Healthy | Cyan `#00FFEE` | All providers healthy |
| Degraded | Yellow `#FFD700` | Warning — provider degraded or quota approaching |
| Error | Red `#FF3333` | Error — failover triggered or provider down |
| Syncing | Purple `#CC44FF` | Active replication in progress |
| Busy | Orange `#FF8800` | Upload or download in progress |

The icon is rendered at runtime via SkiaSharp — no static PNG assets required.

---

## API Integration

The client talks exclusively to the **WEB4 OASIS API** (`NextGenSoftware.OASIS.API.ONODE.WebAPI`):

| Operation | Endpoint |
|---|---|
| Load all items | `POST api/data/load-all-holons` |
| Load single holon | `POST api/data/load-holon` |
| Download file bytes | `POST api/data/load-file` |
| Upload file | `POST api/data/save-file` |
| Save / rename holon | `POST api/data/save-holon` |
| Delete holon | `DELETE api/data/delete-holon` |
| HyperDrive dashboard | `GET api/hyperDrive/dashboard` |
| Provider list | `GET api/hyperDrive/config` |
| Provider metrics | `GET api/hyperDrive/metrics` |
| Avatar search | `GET api/avatar/search?searchQuery=...` |
| Authenticate | `POST api/avatar/authenticate` |

All calls return `OASISResult<T>` with `IsError`, `IsWarning`, `Message`, and `Result` fields.

---

## Configuration

Settings are stored at `%APPDATA%/OasisHyperDriveClient/settings.json` (Windows) or the platform equivalent. They are managed via the in-app Settings window and include:

| Setting | Default | Description |
|---|---|---|
| `ApiBaseUrl` | `https://api.oasis.ac` | ONODE API base URL |
| `DefaultProvider` | _(blank = auto)_ | Preferred provider for uploads |
| `AutoStartOnLogin` | `false` | Create OS autostart entry |
| `Theme` | `Dark` | UI theme |
| `DashboardRefreshSeconds` | `30` | How often to poll the dashboard |
| `Notifications.*` | Various | Per-event toast notification toggles |

---

## Design Documentation

The full design specification is in the OASIS trust repo:

- [`docs/OASIS-HyperDrive-Client-Design-Spec.md`](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-Or-Avatars/blob/master/docs/OASIS-HyperDrive-Client-Design-Spec.md)

---

## Roadmap

- **Phase 1 (complete)** — Tray icon, login, file browser, CRUD, metadata viewer, provider filter, dashboard
- **Phase 2 (complete)** — Upload/download, Send to Avatar, Settings window, OS notifications, auto-start, right-click context menu, SkiaSharp icon renderer
- **Phase 3 (planned)** — Local caching + offline read, batch operations, sharing links, quota breakdown per provider, Velopack installers, certificate pinning

---

## Related Projects

- [OASIS API / ONODE](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-Or-Avatars) — the backend this client talks to
- [ONODE Manager](../ONODEManager/) — companion desktop app for managing ONODE nodes (same Avalonia stack)
