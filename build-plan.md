# CopyX - Build Plan

## Overview

CopyX is a terminal-based file and directory copy utility inspired by `cp`, but with enhanced visibility into the copy process.

### Key Features

- Copy files and directories
- Real-time progress tracking
- Display currently copying file
- Show file count progress
- Show total bytes copied
- Display copy speed
- Error reporting and summary
- Graceful cancellation support

---

# Phase 1: Project Setup

## Create the Project

```bash
dotnet new console -n CopyX
cd CopyX
```

## Install Dependencies

```bash
dotnet add package Spectre.Console
```

### Why Spectre.Console?

Provides:

- Progress bars
- Spinners
- Colored output
- Status displays
- Rich terminal UI

---

# Phase 2: Command-Line Interface

## Initial Command Format

```bash
copyx <source> <destination>
```

### Examples

```bash
copyx file.txt ./backup

copyx ./documents ./backup/documents
```

## Responsibilities

- Parse arguments
- Validate inputs
- Route execution to the correct copy handler

---

# Phase 3: Source Validation

Determine the type of source provided.

### Supported Sources

- Single file
- Directory

### Validation Rules

```text
✓ Source file exists
✓ Source directory exists
✓ Destination path is valid
✓ User has sufficient permissions
```

### Failure Cases

```text
✗ Source does not exist
✗ Invalid path
✗ Access denied
```

---

# Phase 4: Directory Discovery

Before copying a directory:

1. Recursively scan all files
2. Count total files
3. Calculate total bytes
4. Build copy manifest

## CopyItem Model

```csharp
public class CopyItem
{
    public string SourcePath { get; set; } = string.Empty;

    public string DestinationPath { get; set; } = string.Empty;

    public long SizeInBytes { get; set; }
}
```

## Example Output

```text
Scanning directory...

Files Found: 2,341
Total Size: 4.2 GB
```

---

# Phase 5: Copy Engine

## Goals

- Preserve directory structure
- Track byte-level progress
- Support cancellation

## Copy Strategy

Use streams instead of `File.Copy`.

### Benefits

- Real-time progress updates
- Speed calculations
- Better error handling
- Cancellation support

### Example

```csharp
var buffer = new byte[1024 * 1024];

while ((bytesRead = await sourceStream.ReadAsync(buffer)) > 0)
{
    await destinationStream.WriteAsync(
        buffer.AsMemory(0, bytesRead));

    copiedBytes += bytesRead;
}
```

---

# Phase 6: Progress Tracking

## Display Current File

```text
Copying:
src/images/logo.png
```

## Display Overall Progress

```text
[████████████████░░░░] 78%
```

## Display Statistics

```text
Files: 156 / 200

Data: 1.2 GB / 1.6 GB

Speed: 45 MB/s
```

## Progress Metrics

Track:

- Total files copied
- Total bytes copied
- Current file progress
- Transfer speed
- Estimated time remaining

---

# Phase 7: Directory Structure Preservation

## Source

```text
src/
├── docs
│   └── report.pdf
└── images
    └── logo.png
```

## Destination

```text
backup/
├── docs
│   └── report.pdf
└── images
    └── logo.png
```

## Relative Path Calculation

```csharp
var relativePath =
    Path.GetRelativePath(sourceRoot, file);

var destinationPath =
    Path.Combine(destinationRoot, relativePath);
```

---

# Phase 8: Optional Features

## Overwrite Existing Files

```bash
copyx src dest --overwrite
```

## Skip Existing Files

```bash
copyx src dest --skip-existing
```

## Dry Run

```bash
copyx src dest --dry-run
```

Outputs what would be copied without performing the operation.

## Verbose Mode

```bash
copyx src dest --verbose
```

Displays every copied file.

---

# Phase 9: Error Handling

## Expected Errors

### File System

- File already exists
- File in use
- Path too long
- Access denied
- Insufficient disk space

### Runtime

- Unexpected stream failures
- Cancellation events

## Error Strategy

Do not stop the entire operation because of a single file failure.

Continue copying and record failures.

### Example

```text
Copied: 198 files

Skipped: 4 files

Failed: 2 files
```

---

# Phase 10: Cancellation Support

Support:

```text
CTRL + C
```

## Behavior

```text
Cancelling...

Copied 342 of 1200 files.

Operation stopped safely.
```

## Implementation

```csharp
using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};
```

---

# Phase 11: Summary Report

After completion:

```text
================================

Copy Completed

Files Copied : 2,341
Files Skipped: 12
Files Failed : 3

Total Size   : 4.2 GB
Duration     : 01:52:33
Average Speed: 38 MB/s

================================
```

---

# Phase 12: Packaging

## Publish

```bash
dotnet publish -c Release
```

## Package as a Global Tool

```bash
dotnet pack
```

Install:

```bash
dotnet tool install \
  --global \
  --add-source ./nupkg \
  CopyX
```

Usage:

```bash
copyx ./source ./destination
```

---

# Suggested Architecture

```text
CopyX/

├── Program.cs

├── Models/
│   ├── CopyItem.cs
│   ├── CopyOptions.cs
│   └── CopyResult.cs

├── Services/
│   ├── FileScanner.cs
│   ├── CopyService.cs
│   ├── ProgressReporter.cs
│   └── StatisticsService.cs

├── Utilities/
│   ├── PathHelper.cs
│   └── SizeFormatter.cs

└── Extensions/
```

---

# MVP Scope

The first release should include:

- Single file copy
- Directory copy
- Recursive scanning
- Progress bar
- Current file display
- File count tracking
- Error handling
- Cancellation support

Future releases can introduce:

- Parallel copy
- Resume support
- Sync mode
- Verification mode
- Network copy optimization
- TUI dashboard
- File filtering and exclusions
