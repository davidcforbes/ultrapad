# Next Steps - WinUI 3 Migration

## Quick Start

1. **Open Visual Studio 2022:**
   ```
   WordPad.sln
   ```

2. **Select Platform:** x64 (recommended) or ARM64

3. **Build:** Press `Ctrl+Shift+B` or F5

## Expected Issues

### XAML Compiler
The XAML compiler may fail on first build in VS2022. This is normal for large migrations.

**Solution:**
- Clean solution: `Build → Clean Solution`
- Rebuild: `Build → Rebuild Solution`
- If issues persist, close VS2022 and delete `obj/` and `bin/` folders

### Missing References
If you see namespace errors after opening in VS2022:

**Solution:**
- Right-click solution → Restore NuGet Packages
- Build → Rebuild Solution

## Testing Priority

### Critical (Test First)
1. Application launches
2. File → Open (all formats)
3. File → Save
4. Basic text editing
5. Keyboard shortcuts (Ctrl+B, Ctrl+S, etc.)

### Important
6. Formatting (bold, italic, fonts)
7. Find/Replace
8. Insert image
9. Print preview
10. Settings persistence

### Nice to Have
11. Theme switching
12. Recent files
13. Spell check integration

## Known Limitations

### Temporarily Disabled
- **Sharing/DataTransferManager** - Line 167-169 in MainWindow.xaml.cs
- **Window close confirmation** - Handled in individual close operations only

### Needs Windows 11
- `AppWindow.Closing` event (for alt pre-close confirmation)

## Quick Fixes

### If Build Fails with "MCG0004" or similar:
```powershell
# Delete generated files
Remove-Item -Recurse -Force obj, bin
# Rebuild
dotnet clean
dotnet build -p:Platform=x64
```

### If File Pickers Don't Work:
Check that window handle is initialized:
```csharp
_hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
```

### If Title Bar Doesn't Show:
Verify in MainWindow constructor:
```csharp
this.SetTitleBar(AppTitleBar);
```

## Success Criteria

✅ Application builds without errors
✅ Launches and shows main window
✅ Can open and edit .rtf files
✅ Can save files
✅ Keyboard shortcuts work

## Documentation

See `WINUI3_MIGRATION_COMPLETE.md` for full migration details.

## Support

If you encounter issues:
1. Check Visual Studio Error List (View → Error List)
2. Review `WINUI3_MIGRATION_COMPLETE.md` - Known Issues section
3. Check Windows Event Viewer for crash details
