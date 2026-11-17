# Stack

## Used

| Tool | Why |
|---|---|
| .NET 10 (LTS) | `net10.0`, nullable reference types; strictly managed SDK. |
| dotnet CLI | Standard build tool with shared settings in `Directory.Build.props`. |
| dotnet format | Enforces code style and whitespace without configuration arguments. |
| xUnit + FsCheck | Enables both unit and property-based testing. |
| NuGet lock files | `packages.lock.json` ensures reproducible builds. |

## Discarded

| Tool | Why not |
|---|---|
| AnalysisMode All | Too many style-only rules that generate noise in clean, simple code. |
| Central package management | The project is small (one library, one test suite); individual files are cleaner. |
| Shell scripts | C# `scripts/Check` is cross-platform and easier to debug for .NET projects. |