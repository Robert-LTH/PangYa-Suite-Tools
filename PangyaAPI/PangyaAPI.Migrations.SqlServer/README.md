# SQL Server migrations

This project contains SQL Server-only EF Core migrations. The server does not apply migrations at startup.

Before generating the baseline, put a working local SQL Server connection in
`../PangyaAPI.Network/appsettings.json` or override `ConnectionStrings__Pangya` in the environment. Never commit credentials.

Run `dotnet tool restore` once to install the repository-pinned EF CLI.

The authoritative baseline must be scaffolded from the live database before `InitialBaseline` is generated. EF scaffolding does not preserve stored procedures, so script procedure and function definitions separately into migration SQL resources.

```powershell
dotnet ef dbcontext scaffold Name=ConnectionStrings:Pangya Microsoft.EntityFrameworkCore.SqlServer --project PangyaAPI/PangyaAPI.SQL --startup-project PangyaAPI/PangyaAPI.Migrations.SqlServer --context PangyaDbContext --schema pangya --no-onconfiguring
dotnet ef migrations add InitialBaseline --project PangyaAPI/PangyaAPI.Migrations.SqlServer --startup-project PangyaAPI/PangyaAPI.Migrations.SqlServer --context PangyaDbContext
dotnet ef migrations script 0 InitialBaseline --project PangyaAPI/PangyaAPI.Migrations.SqlServer --startup-project PangyaAPI/PangyaAPI.Migrations.SqlServer --context PangyaDbContext --output artifacts/InitialBaseline.sql
```

Do not run `database update` against the source database. Validate the generated schema and stored-procedure scripts against an isolated empty database first. Adoption by an existing database requires a reviewed, manual insertion into `__EFMigrationsHistory`; no application code performs that operation.
