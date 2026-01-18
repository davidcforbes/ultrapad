# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

UltraPad is a modern unofficial replacement for Windows WordPad, built as a Universal Windows Platform (UWP) application using C# and XAML. It provides rich text editing functionality with a Windows 11-style design.

## Build Requirements

- Windows 11 24H2 or newer
- Visual Studio 2026 insider version (Community edition is sufficient)
- WinUI app development workload installed
- Windows 11 SDK 26000

## Building and Running

Open the solution file and build:
```
WordPad.sln
```

The main project is `WordPadApp.csproj` located in the `WordPad/` directory.

**Build in Visual Studio**: Open `WordPad.sln` in Visual Studio and press F5 to build and run in Debug mode.

**Platform targets**: The project supports x86, x64, ARM, and ARM64 platforms.

**Configuration notes**:
- Debug builds use `bin\{Platform}\Debug\`
- Release builds use `bin\{Platform}\Release\` with .NET Native toolchain enabled

## Architecture

### Entry Point and Application Lifecycle
- **App.xaml.cs**: Application entry point, handles app initialization, suspension, and global exception handling. Contains static `FontClass` reference.
- **MainPage.xaml/cs**: Main editor page with ribbon interface, handles document operations, file I/O, and text editing.

### Core Components

**Editor**:
- `Editor.cs`: The RichEditBox control wrapper
- `MainPage.xaml.cs`: Contains the main editing logic (~2500+ lines), handles file operations, formatting, and UI coordination

**Ribbon UI** (`WordPadUI/Ribbon/`):
- `FontToolbar.xaml/cs`: Font selection, sizing, and basic character formatting (bold, italic, underline, strikethrough, subscript, superscript)
- `ParagraphToolbar.xaml/cs`: Paragraph alignment, indentation, line spacing, and list formatting
- `EditingToolbar.xaml/cs`: Cut, copy, paste, undo, redo, find/replace operations
- `InsertToolbar.xaml/cs`: Insert images, Paint objects, dates, and other objects

**UI Components** (`WordPadUI/`):
- `TextRuler.xaml/cs`: Text margin ruler control for document layout
- `DocTree.xaml/cs`: Document tree/outline view
- `Paragraph.xaml/cs`: Paragraph formatting dialog
- `Pageprop.xaml/cs`: Page properties dialog
- `TabsDialog.xaml/cs`: Tab stops configuration
- `Settings/SettingsPage.xaml/cs`: Application settings interface

### Helper Classes (`Helpers/`)

- **FileFormatsHelper.cs**: Handles conversion between document formats (notably DOCX to RTF conversion)
- **OdtHelper.cs**: ODT (OpenDocument Text) file format support
- **RichEditHelpers.cs**: Extension methods for RichEditBox - font sizing, find/replace, text alignment, character formatting. Contains `AlignMode` and `FormattingMode` enums.
- **RecentlyUsedHelper.cs**: Manages recent files list
- **SettingsManagerMain.cs**: Manages application settings persistence via ApplicationData
- **UnitManager.cs**: Handles unit conversions (e.g., pixels to inches)
- **ZoomConverter.cs**: Zoom level value converter for UI binding

### Data Classes (`Classes/`)

- **FontClass.cs**: Font metadata
- **CUnit.cs**: Custom unit representation

## File Format Support

The application uses several file format handlers:
- RTF: Native support via Windows RichEditBox
- DOCX: Via DocumentFormat.OpenXml NuGet package (conversion to RTF)
- ODT: Via custom OdtHelper
- TXT: Plain text support

## Key Dependencies

From `WordPadApp.csproj`:
- `Microsoft.NETCore.UniversalWindowsPlatform` (6.2.14)
- `Microsoft.UI.Xaml` (2.8.2) - WinUI 2 controls
- `Microsoft.Toolkit.Uwp` (7.1.3) - UWP Community Toolkit
- `DocumentFormat.OpenXml` (2.20.0) - DOCX support
- `Fluent.Icons` (1.1.110) - Icon library
- `FanKit.Transformers` (1.6.4)

## State Management

- Uses ApplicationDataContainer (`localSettings`) for persisting user preferences
- `SettingsManagerMain.cs` initializes defaults and loads/saves settings
- Document state tracked through `saved` boolean flag and `originalDocText` comparison
- Recent files managed through `RecentlyUsedHelper` and `RecentlyUsedViewModel`

## Namespace Notes

The project uses the namespace `RectifyPad` for the main application code, despite the assembly name being `WordPad`. Be aware of this inconsistency when navigating the codebase.

## Packaging

The `Appx2Msi/` directory contains tooling to convert the APPX package to MSI format using WiX Toolset. This enables sideloading and certificate trust configuration for deployment.

## Theming

The application supports both light and dark themes:
- Theme assets stored in `Assets/theme-light/` and `Assets/theme-dark/`
- Theme loading handled in MainPage constructor via `LoadThemeFromSettings()`
- Custom icon font: `CustomIconFont` resource (WordPadIcons.ttf)

## Important Implementation Details

1. **RichEditBox Extensions**: Use extension methods in `RichEditHelpers.cs` for text manipulation operations rather than direct API calls
2. **File Operations**: File I/O goes through `MainPage.xaml.cs` methods, using Windows.Storage APIs
3. **Ribbon Connection**: `ConnectRibbonToolbars()` in MainPage initialization connects toolbar controls to the main editor
4. **Zoom**: Managed through `ZoomSlider` with predefined options in `ZoomOptions` collection
5. **Title Bar**: Custom title bar with `ExtendViewIntoTitleBar = true` for modern appearance
