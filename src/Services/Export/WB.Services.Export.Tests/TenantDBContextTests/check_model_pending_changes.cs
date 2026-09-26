using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Services.Export.Tests;

namespace WB.Services.Scheduler.Tests.TenantDbContextTests;

[TestFixture]
public class check_model_pending_changes
{
    [Test]
    public void should_suppress_pending_model_changes_warning_for_tenant_specific_schema()
    {
        var fakeConnectionString = "Host=localhost;Port=9999;Username=fake_user;Database=fake_db;";
        var context = Create.NpgsqlTenantDbContext(fakeConnectionString, tenantId: new("tenant-id"), tenantName: "test");

        // triggers OnConfiguring where the warning suppression is configured
        var _ = context.ChangeTracker;

        var warningsConfiguration = context
            .GetService<IDbContextOptions>()
            .FindExtension<CoreOptionsExtension>()!
            .WarningsConfiguration;

        var behavior = warningsConfiguration.GetBehavior(RelationalEventId.PendingModelChangesWarning);

        ClassicAssert.AreEqual(WarningBehavior.Ignore, behavior,
            "TenantDbContext maps each tenant to its own schema at runtime, so EF Core always reports "
            + "pending model changes against the schema-agnostic snapshot. The PendingModelChangesWarning "
            + "must be suppressed to avoid noise/exceptions when the export service runs migrations.");
    }

    [Test]
    public void should_keep_default_pending_model_changes_warning_for_schema_agnostic_context()
    {
        var fakeConnectionString = "Host=localhost;Port=9999;Username=fake_user;Database=fake_db;";
        var context = Create.NpgsqlTenantDbContext(fakeConnectionString, tenantName: "test");

        // triggers OnConfiguring so the warning behavior can be inspected
        var _ = context.ChangeTracker;

        var warningsConfiguration = context
            .GetService<IDbContextOptions>()
            .FindExtension<CoreOptionsExtension>()!
            .WarningsConfiguration;

        var behavior = warningsConfiguration.GetBehavior(RelationalEventId.PendingModelChangesWarning);

        ClassicAssert.AreEqual(WarningBehavior.Throw, behavior);
    }
}
