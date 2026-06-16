# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Created `VisibilityDiagnostic` base project targeting Revit 2024 (.NET 4.8).
- Implemented `VisibilityChecker.cs` to test multiple visibility conditions:
  - Hidden by Category (V/G)
  - Hidden in View (by Element)
  - Hidden by Workset
  - Hidden by View Filter
  - Hidden by Temporary Hide/Isolate
  - Center point outside of Crop Region bounds
- Added Modeless UI (`DiagnosticWindow.xaml`) to allow real-time checking alongside the active Revit document.
- Implemented `DiagnosticEventHandler` (`IExternalEventHandler`) to safely interact with the Revit API from the modeless WPF window.
- Added a `library/` folder containing the Revit API DLLs, configured in the `.csproj` file to allow for easier compilation without requiring the exact Revit installation path to be modified locally.
- Added a custom Ribbon Tab (`BIMTools`) and Panel (`Diagnostics`) to launch the tool via `App.cs` (`IExternalApplication`).
- Added color highlighting for diagnostic results to easily identify visibility issues (Red for issues, Green for passed checks).

### Changed
- Refactored `Command.cs` to display a modeless window instead of a modal one.
- Registered the Add-in as an Application instead of a standalone External Tool command to keep the Add-Ins tab clean.
- Filtered the Views ComboBox to exclude non-graphical views (Schedules, Legends, System Browsers).
- Modified the Views ComboBox to display the `ViewType` next to the `ViewName` to prevent confusion between views with the same name.
