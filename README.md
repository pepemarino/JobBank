# JobBank — Local setup

This project reads the `EmploymentBankContext` connection string from configuration. Do not commit secrets into source control. Use one of the options below to provide the connection string locally.

## Quick options

1. Use `launchSettings.json` (recommended for quick VS debugging)
   - Copy the template:
     - Windows / macOS / Linux:
       - `cp Properties/launchSettings.json.template Properties/launchSettings.json`
   - Edit `Properties/launchSettings.json` and replace `<PUT_LOCAL_CONNSTR_HERE_OR_USE_USER_SECRETS>` with your connection string.
   - Visual Studio and `dotnet run` (when launched from the IDE profile) will pick it up.

2. Use environment variable (works for `dotnet run`, CI, containers)
   - Name: `ConnectionStrings__EmploymentBankContext` (two underscores)
   - PowerShell (session):
     - `$env:ConnectionStrings__EmploymentBankContext = "Data Source=...;Initial Catalog=JobBank;..."`
     - `dotnet run`
   - CMD (session):
     - `set ConnectionStrings__EmploymentBankContext=Data Source=...;Initial Catalog=JobBank;...`
     - `dotnet run`
   - Bash / macOS:
     - `export ConnectionStrings__EmploymentBankContext='Data Source=...;Initial Catalog=JobBank;...'`
     - `dotnet run`

3. Use `dotnet user-secrets` (best for local development without committing)
   - From the project folder:
     - `dotnet user-secrets init`
     - `dotnet user-secrets set "ConnectionStrings:EmploymentBankContext" "Data Source=...;Initial Catalog=JobBank;..."`

## Run
- Development (CLI): ensure env var or launchSettings is set, then `dotnet run`
- Visual Studio: use the appropriate launch profile (IIS Express or JobBank)

## Security notes
- Add `Properties/launchSettings.json` to `.gitignore` and commit only the template.
- Rotate any credentials accidentally committed to the remote.
- For production, use managed secrets (Azure Key Vault, environment variables in hosting infra).