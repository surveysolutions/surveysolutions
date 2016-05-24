using System.ComponentModel;
using System.Threading.Tasks;
using NConsole;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace dbup
{
    [Description("Updates plain storage to a state of 5.10 release for later migrations")]
    internal class HqUpdatePlain : IConsoleCommand
    {
        [Description("Plain store connection string")]
        [Argument(Name = "cs")]
        public string ConnectionString { get; set; }

        public async Task RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = this.ConnectionString;
                db.Dialect<PostgreSQL91Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
            });
            cfg.SetProperty(Environment.WrapResultSets, "true");
            cfg.AddDeserializedMapping(MappingsCollector.GetPlainMappingsForHQ(), "Main");
            var update = new SchemaUpdate(cfg);
            update.Execute(true, true);

            await DbMarker.MarkAsZeroMigrationDone(this.ConnectionString);
        }
    }
}