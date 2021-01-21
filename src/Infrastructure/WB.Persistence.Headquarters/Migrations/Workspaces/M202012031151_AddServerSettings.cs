using System;
using Dapper;
using FluentMigrator;
using Microsoft.Extensions.Logging;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2020_12_03_11_51)]
    public class M202012031151_AddServerSettings : ForwardOnlyMigration
    {
        private const string ServerSettingsTableName = "server_settings";
        private readonly ILogger logger;

        public M202012031151_AddServerSettings(ILoggerProvider logger)
        {
            this.logger = logger.CreateLogger(GetType().Name);
        }
        
        public override void Up()
        {
            Create.Table(ServerSettingsTableName)
                .WithColumn("id").AsString().NotNullable().PrimaryKey()
                .WithColumn("value").AsString().NotNullable();

            Execute.WithConnection((connection, transaction) =>
            {
                void Execute(string sql)
                {
                    logger.LogInformation(sql);
                    connection.Execute(sql);
                }
                
                if (connection.IsTableExistsInSchema("ws_primary", "appsettings"))
                {
                    Execute($@"INSERT INTO {ServerSettingsTableName} (id, value)
                SELECT 'TenantPublicId', value ->> 'TenantPublicId' as value from  ws_primary.appsettings a 
                       where a.id = 'TenantSettings'
                ON CONFLICT (id) DO NOTHING;
                DELETE FROM ws_primary.appsettings where id = 'TenantSettings'");
                }
                else
                {
                    var serviceKey = Guid.NewGuid();
                    Execute($@"INSERT INTO {ServerSettingsTableName} (id, value) VALUES ('TenantPublicId', '{serviceKey:N}')
                               ON CONFLICT (id) DO NOTHING;");
                }
            });
        }
    }
}
