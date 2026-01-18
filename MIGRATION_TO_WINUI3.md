# Migration Path from UWP to Windows App SDK / WinUI 3

## Executive Summary

UltraPad is currently built on the Universal Windows Platform (UWP), which was officially deprecated by Microsoft in October 2021. This document outlines the migration path to Windows App SDK with WinUI 3, which is the recommended modern platform for Windows desktop applications.

## Why Migrate?

### UWP Deprecation Status
- **Deprecated**: October 2021
- **Current Support**: Bug fixes, reliability patches, and security updates only
- **No New Features**: Microsoft has confirmed no new features will be added to UWP
- **Future Platform**: Windows App SDK + WinUI 3 is Microsoft's strategic direction

### Benefits of Windows App SDK / WinUI 3
1. **Active Development**: Regular feature updates and improvements
2. **Better Desktop Integration**: Enhanced access to Win32 APIs
3. **Improved Performance**: Better rendering and resource management
4. **Modern UI Controls**: Latest Fluent Design components
5. **Long-term Support**: Microsoft's commitment to the platform

## Current Technology Stack

### UltraPad (UWP)
- **Framework**: UWP (Universal Windows Platform)
- **UI**: WinUI 2.8.2 (maintenance mode)
- **Target Platform**: Windows 10 17763+ / Windows 11
- **.NET**: .NET UWP 6.2.14
- **Community Toolkit**: 7.1.3 (archived, no longer maintained)

## Migration Strategy

### Phase 1: Assessment & Planning (Weeks 1-2)

#### Compatibility Analysis
1. **Review Current Dependencies**
   - DocumentFormat.OpenXml 2.20.1 ✓ Compatible
   - Microsoft.Toolkit.Uwp 7.1.3 ⚠️ Needs migration to CommunityToolkit.WinUI
   - WinUI 2.8.2 ⚠️ Migrate to WinUI 3
   - FanKit.Transformers 1.6.4 ❓ Check compatibility

2. **Identify UWP-Specific APIs**
   - File pickers (Windows.Storage.Pickers)
   - Settings storage (ApplicationData)
   - Recent files (StorageApplicationPermissions)
   - File activation handlers

3. **Catalog Custom Controls**
   - RichEditBox usage
   - TextRuler control
   - Custom ribbon implementation
   - Toolbar controls

#### Migration Approach
**Recommended**: Incremental migration with parallel development

1. Create new Windows App SDK project
2. Port core functionality module by module
3. Maintain UWP version during transition
4. Test thoroughly on Windows 10 and 11
5. Release WinUI 3 version when feature-complete

### Phase 2: Project Setup (Weeks 3-4)

#### Create Windows App SDK Project
```powershell
# Using Visual Studio 2022
# File -> New -> Project -> Windows App SDK
# Template: "Blank App, Packaged (WinUI 3 in Desktop)"
```

#### Update Project Structure
```
UltraPad-WinUI3/
├── UltraPad/              # Main application
│   ├── App.xaml
│   ├── MainWindow.xaml     # Replaces MainPage.xaml
│   ├── Helpers/
│   ├── Views/
│   └── Controls/
├── Package.appxmanifest
└── UltraPad.sln
```

#### Configure Package References
```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.6.*" />
<PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.*" />
<PackageReference Include="CommunityToolkit.WinUI.Controls" Version="8.1.*" />
<PackageReference Include="CommunityToolkit.WinUI.UI" Version="8.1.*" />
<PackageReference Include="DocumentFormat.OpenXml" Version="3.2.*" />
```

### Phase 3: Code Migration (Weeks 5-12)

#### Namespace Changes
```csharp
// UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;

// Windows App SDK
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;  // Still available
using Windows.Storage.Pickers;  // Still available
```

#### Key API Migrations

##### Window Management
```csharp
// UWP
ApplicationView.GetForCurrentView().Title = "UltraPad";

// WinUI 3
Window.Current.Title = "UltraPad";
```

##### File Pickers
```csharp
// WinUI 3 requires window handle
var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
var picker = new FileOpenPicker();
WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
```

##### Settings Storage
```csharp
// Both use same API - no changes needed
ApplicationData.Current.LocalSettings
```

##### Theming
```csharp
// UWP
if (Window.Current.Content is FrameworkElement rootElement)
    rootElement.RequestedTheme = ElementTheme.Dark;

// WinUI 3
if (Content is FrameworkElement rootElement)
    rootElement.RequestedTheme = ElementTheme.Dark;
```

#### Control Migrations

##### RichEditBox
- ✓ Available in WinUI 3 without changes
- Same Windows.UI.Text namespace

##### Custom Controls
- TextRuler: Requires XAML updates for WinUI 3
- Ribbon components: May need modernization
- Status bar: Minor updates needed

#### Breaking Changes to Address

1. **App.xaml.cs Structure**
   - No more `OnLaunched` override
   - Use `App()` constructor and `OnLaunched` event
   - Window activation differs

2. **Title Bar Customization**
   ```csharp
   // WinUI 3 approach
   AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
   ```

3. **ApplicationDataContainer**
   - Same API, no changes needed

4. **File Associations**
   - Update Package.appxmanifest format
   - Activation handling changes slightly

### Phase 4: UI/UX Updates (Weeks 13-16)

#### Modernize with WinUI 3 Controls
- Update to latest Fluent Design components
- Use new Microsoft.UI.Xaml.Controls
- Implement Mica/Acrylic materials
- Update navigation patterns if needed

#### Accessibility
- Test with Narrator
- Verify keyboard navigation
- Ensure high contrast mode support

### Phase 5: Testing (Weeks 17-20)

#### Test Matrix
| Scenario | Windows 10 22H2 | Windows 11 24H2 |
|----------|-----------------|-----------------|
| File Open/Save | ✓ | ✓ |
| RTF Editing | ✓ | ✓ |
| DOCX Import | ✓ | ✓ |
| ODT Import | ✓ | ✓ |
| Printing | ✓ | ✓ |
| Themes | ✓ | ✓ |
| File Associations | ✓ | ✓ |

#### Performance Testing
- Startup time
- Document load time
- Memory usage
- Rendering performance

#### Security Verification
- File system access working correctly
- No security regressions
- Certificate validation

### Phase 6: Deployment (Weeks 21-22)

#### Packaging Options
1. **MSIX Package** (Recommended)
   - Self-updating via Microsoft Store
   - Clean install/uninstall
   - Automatic updates

2. **Unpackaged Deployment**
   - For enterprise scenarios
   - No Store requirement
   - Manual updates

#### Distribution Channels
- Microsoft Store
- GitHub Releases
- Direct download from website

## Migration Checklist

### Prerequisites
- [ ] Visual Studio 2022 17.4 or later
- [ ] Windows App SDK 1.6 or later
- [ ] Windows 11 SDK (10.0.22000.0 or later)

### Code Migration
- [ ] Create new WinUI 3 project
- [ ] Migrate App.xaml and App.xaml.cs
- [ ] Convert MainPage to MainWindow
- [ ] Update all namespaces
- [ ] Migrate file I/O operations
- [ ] Update window management code
- [ ] Migrate custom controls
- [ ] Update resource dictionaries
- [ ] Migrate settings management
- [ ] Update theme handling

### Testing
- [ ] Unit tests pass
- [ ] Manual testing complete
- [ ] Performance benchmarks met
- [ ] Accessibility verified
- [ ] Windows 10 compatibility confirmed
- [ ] Windows 11 features working

### Documentation
- [ ] Update README.md
- [ ] Update build instructions
- [ ] Document new dependencies
- [ ] Create migration notes
- [ ] Update user documentation

### Deployment
- [ ] Create MSIX package
- [ ] Test installation
- [ ] Prepare Store listing
- [ ] Plan rollout strategy

## Risks & Mitigation

### Risk 1: Breaking Changes in Dependencies
**Mitigation**: Thoroughly test each dependency upgrade in isolation

### Risk 2: User Disruption
**Mitigation**: Maintain UWP version until WinUI 3 version is stable

### Risk 3: Performance Regression
**Mitigation**: Establish performance baselines before migration

### Risk 4: Windows 10 Compatibility
**Mitigation**: Test extensively on Windows 10 22H2

## Timeline Estimate

| Phase | Duration | Effort |
|-------|----------|--------|
| Assessment | 2 weeks | Medium |
| Project Setup | 2 weeks | Low |
| Code Migration | 8 weeks | High |
| UI/UX Updates | 4 weeks | Medium |
| Testing | 4 weeks | High |
| Deployment | 2 weeks | Medium |
| **Total** | **22 weeks** | **~6 person-months** |

## Resources

### Microsoft Documentation
- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/)
- [WinUI 3](https://learn.microsoft.com/windows/apps/winui/winui3/)
- [Migration Guide](https://learn.microsoft.com/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/overall-migration-strategy)

### Community Resources
- [WinUI Gallery](https://github.com/microsoft/WinUI-Gallery)
- [Windows Community Toolkit](https://github.com/CommunityToolkit/Windows)
- [Template Studio](https://github.com/microsoft/TemplateStudio)

## Conclusion

While the migration from UWP to Windows App SDK/WinUI 3 represents significant effort, it is necessary for the long-term viability of UltraPad. The migration will:

1. Ensure continued platform support from Microsoft
2. Enable access to modern Windows features
3. Provide a better user experience with updated UI controls
4. Maintain security through ongoing platform updates

The incremental migration strategy minimizes risk while allowing the project to modernize at a sustainable pace.
