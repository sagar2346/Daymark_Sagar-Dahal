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

## Folder Structure

- **Models/**: Contains `JournalEntry.cs` (Data Model).
- **Services/**: Contains `ExportService.cs` (PDF Generation Logic).
- **ViewModels/**: Contains `SettingsViewModel.cs` (Business Logic) and `BaseViewModel.cs`.
- **Views/**: Contains `SettingsPage.xaml` and `SettingsPage.xaml.cs` (UI Layer).

## Integration Guide

To use these in your own .NET MAUI project:
1. Install NuGet packages: `CommunityToolkit.Mvvm`, `QuestPDF`, and `sqlite-net-pcl`.
2. Register the services and viewmodels in `MauiProgram.cs`.
3. Copy the files into your project, keeping the namespace structure or updating it as needed.
