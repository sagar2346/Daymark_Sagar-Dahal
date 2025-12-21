# Daily Journal Functionalities

This repository contains specific functionalities extracted from the Daily Journal App.

## Features Included

### 1. Dark Mode Toggle
- **Logic**: Implemented in `SettingsViewModel.cs` using `CommunityToolkit.Mvvm`.
- **UI**: Implemented in `SettingsPage.xaml` with a `Switch` control and `AppThemeBinding`.
- **Persistence**: Theme preference is saved using `Maui.Storage.Preferences`.

### 2. PDF Export
- **Service**: `ExportService.cs` uses the `QuestPDF` library to generate professional PDF documents from journal entries.
- **Trigger**: The `ExportDataCommand` in `SettingsViewModel.cs` fetches entries and saves the PDF to the user's Documents folder.

