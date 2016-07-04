using System.ComponentModel;
using System.Threading.Tasks;
using NConsole;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;
using Environment = NHibernate.Cfg.Environment;

namespace dbup
{
    [Description("Updates read side to a state of 5.10 release for later migrations")]
    internal class HqUpdateReadSide : IConsoleCommand
    {
        [Description("Read side connection string")]
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
            cfg.AddDeserializedMapping(MappingsCollector.GetReadSideForHq(), "Main");
            var update = new SchemaUpdate(cfg);
            update.Execute(true, true);

            await DbMarker.MarkAsZeroMigrationDone(this.ConnectionString);
        }
    }
}