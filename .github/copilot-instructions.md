# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules
- Repository cloned at C:\Development\ultrapad on branch main with remote origin https://github.com/davidcforbes/ultrapad
- Visual Studio install path for user: C:\Program Files\Microsoft Visual Studio\18\Insiders with MSVC versions installed under VC\Tools\MSVC.

## Build Instructions

### Building MSIX Package

Use Visual Studio Insiders MSBuild to build the MSIX package:

```powershell
cd C:\Development\ultrapad
& "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe" WordPad\WordPadApp.csproj /p:Configuration=Release /p:Platform=x64 /p:GenerateAppxPackageOnBuild=true /restore
```

### Signing the MSIX Package

After building, sign the package with the test certificate:

```powershell
& "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe" sign /fd SHA256 /a /f "WordPad\UltraPad_TemporaryKey.pfx" /p "password123" "WordPad\bin\x64\Release\net6.0-windows10.0.19041.0\AppPackages\WordPadApp_11.2311.2.0_Test\WordPadApp_11.2311.2.0_x64.msix"
```

### Installing the Package

To install the signed MSIX package locally:

```powershell
cd "C:\Development\ultrapad\WordPad\bin\x64\Release\net6.0-windows10.0.19041.0\AppPackages\WordPadApp_11.2311.2.0_Test"
.\Add-AppDevPackage.ps1
```

The script will automatically:
- Install the test certificate to Trusted Root Certification Authorities
- Install the MSIX package

### Certificate Information
- Certificate file: `WordPad\UltraPad_TemporaryKey.pfx`
- Certificate password: `password123` (for local development only)
- Public certificate: `WordPad\UltraPad_TemporaryKey.cer`
- Publisher: CN=lixkote

### Project Configuration
- MSIX tooling is enabled in WordPadApp.csproj
- Package signing is disabled during build (signed manually after)
- WindowsPackageType is set to MSIX
- Use SettingsStorage.SetValue() / GetValue() for settings (not settings.Values[])