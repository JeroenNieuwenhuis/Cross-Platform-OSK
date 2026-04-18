# Typo

Cross-platform on-screen keyboard built with Avalonia.

## Fedora

The app can be built and launched on Fedora with the .NET 9 SDK:

```bash
sudo dnf install dotnet-sdk-9.0
dotnet run
```

Notes:

- The current Linux path is conservative and prioritizes launching cleanly on Fedora.
- Advanced window-manager behavior is Windows-only right now.
- Synthetic key injection is also Windows-only right now, so the UI will run on Fedora but key output is not implemented for Linux yet.
- Fedora defaults to Wayland on many systems; if Linux input injection is added later, it will likely need an X11 session or a Wayland-specific backend.
