using System;
using System.Configuration;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using NUnit.Framework;
using Polly;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Scheduler.Tests.TenantDbContextTests;

[TestFixture]
public class check_model_pending_changes
{
    protected IConfiguration Configuration => new ConfigurationBuilder()
        .AddJsonFile($@"appsettings.json", true)
        .AddJsonFile($@"appsettings.DEV_DEFAULTS.json", true)
        .AddJsonFile($"appsettings.{Environment.MachineName}.json", true)
        .Build();
    
    private IServiceProvider PrepareOneTime()
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        if (connectionString == null)
            throw new ArgumentNullException(nameof(connectionString));
        
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder();
        connectionStringBuilder.ConnectionString = connectionString;
        connectionStringBuilder.Database = "exports_service_tests_" + Guid.NewGuid().FormatGuid();
        connectionString = connectionStringBuilder.ToString();
        
        var services = new ServiceCollection()
            .AddDbContext<TenantDbContext>(ops =>
            {
                ops.UseNpgsql(connectionString);
                ops.ConfigureWarnings(w => w.Throw(RelationalEventId.PendingModelChangesWarning));
            });
            
        
        var tenantInfo = new TenantInfo
        (
            baseUrl:"",
            id : TenantId.None,
            shortName : ""
        );
        var tenantContext = new TenantContext(null, tenantInfo);
        
        var optionsConnectionsSettings = new Mock<IOptions<DbConnectionSettings>>();
        DbConnectionSettings dbConnectionSettings = new DbConnectionSettings()
        {
            DefaultConnection = connectionString
        };
        optionsConnectionsSettings.Setup(m => m.Value).Returns(() => dbConnectionSettings);
        
        services.AddSingleton<ITenantContext>(tenantContext);
        services.AddSingleton(optionsConnectionsSettings.Object);
        services.AddTransient(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddTransient<DbContextOptions, DbContextOptions<TenantDbContext>>();
        return services.BuildServiceProvider();
    }

    private async Task Init()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            
        using var scope = PrepareOneTime().CreateScope();
        var db = scope.ServiceProvider.GetService<TenantDbContext>();
        
        await db.Database.MigrateAsync();
    }

    [Test]
    //[Ignore("This test is ignored because it is has to befixed first.")]
    public void should_create_db_without_any_exceptions()
    {
        Assert.DoesNotThrowAsync(async () => await Init());
    }
}
