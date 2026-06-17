# CopyX

CopyX is a terminal file and directory copy utility with progress visibility, practical metrics, and a little more life than the traditional copy experience.

The classic `cp` command is dependable, but it can also be painfully quiet. You start a big copy, stare at a blinking cursor, and wonder whether anything is happening, how much is left, how fast it is moving, or whether it hit a problem somewhere in the middle.

CopyX keeps the familiar job simple: copy this source to that destination. The difference is that it plans to make the work observable and reassuring, with progress tracking, file counts, total size, duration, average speed, skipped and failed file counts, and readable terminal output.

## Features

- Copy a single file to a file or directory destination.
- Copy a directory tree recursively.
- See scan results before the copy starts.
- Track copy progress in the terminal.
- Review a metrics summary when the run completes.
- Preview work with `--dry-run`.
- Replace existing files with `--overwrite`.
- Leave existing files untouched with `--skip-existing`.
- Print per-file details with `--verbose`.
- Cancel safely with Ctrl+C.

## Requirements

- .NET 10 SDK

## Usage

```powershell
copyx <source> <destination> [--overwrite|--skip-existing] [--dry-run] [--verbose]
```

### Options

| Option | Description |
| --- | --- |
| `--overwrite` | Replace destination files when they already exist. |
| `--skip-existing` | Skip destination files when they already exist. |
| `--dry-run` | Show what would happen without writing files. |
| `--verbose` | Print additional per-file copy details. |
| `--help`, `-h` | Show usage information. |

`--overwrite` and `--skip-existing` cannot be used together.

## Examples

Copy a single file:

```powershell
copyx .\notes.txt .\backup\notes.txt
```

Copy a file into a destination directory:

```powershell
copyx .\notes.txt .\backup
```

Copy a directory tree:

```powershell
copyx .\photos .\archive\photos
```

Preview a copy without writing anything:

```powershell
copyx .\photos .\archive\photos --dry-run
```

Overwrite files that already exist:

```powershell
copyx .\photos .\archive\photos --overwrite
```

Skip files that already exist:

```powershell
copyx .\photos .\archive\photos --skip-existing
```

Print more detail while copying:

```powershell
copyx .\photos .\archive\photos --verbose
```

## Exit Codes

| Code | Meaning |
| --- | --- |
| `0` | Copy completed successfully, or help was shown. |
| `1` | Invalid arguments or a startup error occurred. |
| `2` | The run completed, but one or more files failed to copy. |
| `130` | The operation was cancelled. |

## Development

Restore dependencies:

```powershell
dotnet restore
```

Build the solution:

```powershell
dotnet build
```

Run tests:

```powershell
dotnet test
```

Run the CLI from source:

```powershell
dotnet run --project src -- <source> <destination>
```

## Local Tool Package

CopyX is configured as a .NET tool package. To pack and install it from a local package output folder:

```powershell
dotnet pack src
dotnet tool install --global --add-source <package-output-folder> CopyX
```

After installation, run it with:

```powershell
copyx <source> <destination>
```
