using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Main.Core;
using Raven.Client.Document;
using Raven.Client.Extensions;

namespace LoadTestDataGenerator
{
    public class LoadTestDataGeneratorRegistry : CoreRegistry
    {
        private readonly string repositoryPath;

        public LoadTestDataGeneratorRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
            this.repositoryPath = repositoryPath;
        }

        public override IEnumerable<Assembly> GetAssweblysForRegister()
        {
            return
                base.GetAssweblysForRegister()
                    .Concat(new[]
                    {
                        typeof(LoadTestDataGeneratorRegistry).Assembly
                    });
        }

        protected override IEnumerable<KeyValuePair<Type, Type>> GetTypesForRegistration()
        {
            return base.GetTypesForRegistration().Concat(new Dictionary<Type, Type>
            {
            });
        }


        protected override void RegisterAdditionalElements()
        {
            base.RegisterAdditionalElements();

            this.Unbind<DocumentStore>();
            var databaseName = ConfigurationManager.AppSettings["Raven.DefaultDatabase"];
            var store = new DocumentStore
                {
                    Url = repositoryPath
                };
            bool isNotSystemDatabase = !string.IsNullOrWhiteSpace(databaseName);
            if (isNotSystemDatabase)
            {
                store.DefaultDatabase = databaseName;
            }
            store.Initialize();
            if (isNotSystemDatabase)
            {
                store.DatabaseCommands.EnsureDatabaseExists(databaseName);
            }
            
            this.Bind<DocumentStore>().ToConstant(store);
        }
    }
}

