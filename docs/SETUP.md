# Installation and Local Setup

Everything needed to get MDK IT Helpdesk running on a fresh machine: install the prerequisites, get the code, create the database, then run the app and the tests.

## 1. Install the prerequisites

| Tool | Why | Check it's installed |
|---|---|---|
| [Git](https://git-scm.com/downloads) | Get the code | `git --version` |
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | Build, run and test the app | `dotnet --version` (10.0 or later) |
| A SQL Server instance | The app's database | see below |

**Database options**, pick one:

- **SQL Server LocalDB (Windows, recommended).** Installed with Visual Studio's *ASP.NET and web development* workload, or on its own by choosing *Download Media → LocalDB* in the [SQL Server Express installer](https://www.microsoft.com/sql-server/sql-server-downloads). The app's default connection string already points at it.
- **SQL Server in Docker (macOS, Linux or Windows).** See [Using Docker instead of LocalDB](#using-docker-instead-of-localdb).

**Visual Studio 2022 (17.13 or later) or 2026** is optional. Earlier versions can't open the `.slnx` solution file.

## 2. Get the code

```bash
git clone https://github.com/bujji1/mdk-it-helpdesk.git
cd mdk-it-helpdesk
```

All of the following commands run from this folder.

## 3. Check LocalDB is running

```bash
sqllocaldb info
```

You should see `MSSQLLocalDB`. If it isn't running:

```bash
sqllocaldb start MSSQLLocalDB
```

## 4. Create the database

```bash
# Download the NuGet packages (needed before the EF Core tools can run)
dotnet restore

# Install the EF Core command-line tool (once per machine)
dotnet tool install --global dotnet-ef --version 10.0.15

# Create the database, apply the migrations and seed the sample data
dotnet ef database update --project src/MdkHelpdesk.Infrastructure --startup-project src/MdkHelpdesk.Web
```

This creates the `MdkHelpdeskDb` database in `(localdb)\MSSQLLocalDB` with 4 helpdesk users and 4 sample tickets.

If `dotnet-ef` is already installed, the install command reports it; that's fine. Run `dotnet tool update --global dotnet-ef --version 10.0.15` to match the version CI uses.

## 5. Run the app

### Option A: command line

```bash
dotnet run --project src/MdkHelpdesk.Web --launch-profile https
```

Then open:

| Page | URL |
|---|---|
| Helpdesk UI | https://localhost:7104 |
| Swagger (API docs) | https://localhost:7104/swagger |
| Health check | https://localhost:7104/health |

If the browser warns about the certificate, trust the .NET development certificate once with `dotnet dev-certs https --trust`, or use the `http` profile instead (http://localhost:5035).

### Option B: Visual Studio

1. Open `MdkHelpdesk.slnx`.
2. Right-click `MdkHelpdesk.Web` → **Set as Startup Project**.
3. Press **F5** to run with the debugger, or **Ctrl+F5** without it.

To browse the data, open **View → SQL Server Object Explorer** and expand `(localdb)\MSSQLLocalDB → Databases → MdkHelpdeskDb`.

## 6. Run the tests

```bash
dotnet test
```

The tests don't need LocalDB: both the unit and integration tests run against in-memory SQLite. In Visual Studio, use **Test → Test Explorer → Run All Tests**.

## Using Docker instead of LocalDB

1. Start SQL Server, choosing your own strong password:

   ```bash
   docker run -d --name mdk-sql -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<your-strong-password>" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
   ```

2. Point the app at it by setting the connection string as an environment variable, so the password never goes into a committed file:

   ```bash
   # macOS / Linux / Git Bash
   export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=MdkHelpdeskDb;User Id=sa;Password=<your-strong-password>;TrustServerCertificate=True"
   ```

   ```powershell
   # PowerShell
   $env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=MdkHelpdeskDb;User Id=sa;Password=<your-strong-password>;TrustServerCertificate=True"
   ```

3. Continue from [step 4](#4-create-the-database) in the same terminal.

## Troubleshooting

| Problem | Fix |
|---|---|
| `Assets file 'project.assets.json' not found` when running `dotnet ef` | Run `dotnet restore` first. |
| `dotnet ef` is not recognized | Install the tool (step 4), then open a new terminal so the tools folder is on your `PATH`. |
| Cannot connect to `(localdb)\MSSQLLocalDB` | Run `sqllocaldb start MSSQLLocalDB`, or install LocalDB (step 1). |
| Visual Studio can't open `MdkHelpdesk.slnx` | Update to Visual Studio 2022 17.13 or later, or use the command line. |
| Browser certificate warning | Run `dotnet dev-certs https --trust`. |
