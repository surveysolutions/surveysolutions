to run application
`dotnet tool install -g FluentMigrator.DotNet.Cli`
`dotnet fm migrate -p Postgres -a %full path to WB.Persistance.Designer.dll% -c "Server=127.0.0.1;Port=5432;User Id=postgres;Password=;Database=Design1;Search Path=plainstore"`
