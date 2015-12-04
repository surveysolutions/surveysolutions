﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Humanizer;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Tool.hbm2ddl;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.Infrastructure.Transactions;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.Infrastructure.Storage.Postgre
{
    public class PostgresReadSideModule : NinjectModule
    {
        internal const string ReadSideSessionFactoryName = "ReadSideSessionFactory";
        internal const string SessionProviderName = "ReadSideProvider";
        private readonly string connectionString;
        private readonly IEnumerable<Assembly> mappingAssemblies;
        private static int memoryCacheSizePerEntity;

        public PostgresReadSideModule(string connectionString, int memoryCacheSizePerEntity,
            IEnumerable<Assembly> mappingAssemblies)
        {
            this.connectionString = connectionString;
            this.mappingAssemblies = mappingAssemblies;
            PostgresReadSideModule.memoryCacheSizePerEntity = memoryCacheSizePerEntity;
        }

        public override void Load()
        {
            this.Kernel.Bind<PostgreConnectionSettings>().ToConstant(new PostgreConnectionSettings{ConnectionString = connectionString });

            this.Kernel.Bind<IPostgresReadSideBootstraper>().To<PostgresReadSideBootstraper>();

            this.Kernel.Bind(typeof(PostgreReadSideRepository<>)).ToSelf().InSingletonScope();
            this.Kernel.Bind(typeof(IReadSideRepositoryWriter<>)).ToMethod(GetReadSideRepositoryWriter).InSingletonScope(); 
            
            this.Kernel.Bind<ISessionFactory>()
                       .ToMethod(kernel => this.BuildSessionFactory())
                       .InSingletonScope()
                       .Named(ReadSideSessionFactoryName);

            this.Kernel.Bind<CqrsPostgresTransactionManager>().ToSelf().InIsolatedThreadScopeOrRequestScopeOrThreadScope();
            this.Kernel.Bind<NoTransactionCqrsPostgresTransactionManager>().ToSelf().InIsolatedThreadScopeOrRequestScopeOrThreadScope();

            this.Kernel.Bind<Func<CqrsPostgresTransactionManager>>().ToMethod(context => () => context.Kernel.Get<CqrsPostgresTransactionManager>());
            this.Kernel.Bind<Func<NoTransactionCqrsPostgresTransactionManager>>().ToMethod(context => () => context.Kernel.Get<NoTransactionCqrsPostgresTransactionManager>());
            
            this.Kernel.Bind<RebuildReadSideCqrsPostgresTransactionManager>().ToSelf();
            this.Kernel.Bind<TransactionManagerProvider>().ToSelf().InSingletonScope();

            this.Kernel.Bind<ISessionProvider>().ToMethod(context => context.Kernel.Get<TransactionManagerProvider>()).Named(SessionProviderName);
            this.Kernel.Bind<ITransactionManager>().ToMethod(context => context.Kernel.Get<CqrsPostgresTransactionManager>());

            this.Kernel.Bind<ITransactionManagerProvider>().ToMethod(context => context.Kernel.Get<TransactionManagerProvider>());
            this.Kernel.Bind<ITransactionManagerProviderManager>().ToMethod(context => context.Kernel.Get<TransactionManagerProvider>());

            this.Kernel.Bind(typeof (IQueryableReadSideRepositoryReader<>)).To(typeof (PostgreReadSideRepository<>));
            this.Kernel.Bind(typeof (IReadSideRepositoryReader<>)).To(typeof (PostgreReadSideRepository<>));
        }

        private ISessionFactory BuildSessionFactory()
        {
            File.WriteAllText(@"D:\Temp\Mapping.xml" ,Serialize(this.GetMappings())); // Can be used to check mappings
            try
            {
                DatabaseManagement.CreateDatabase(connectionString);
            }
            catch (Exception exc)
            {
                this.Kernel.Get<ILogger>().Fatal("Error during db initialization.", exc);
                throw;
            }
            
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = connectionString;
                db.Dialect<PostgreSQL82Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
            });
            cfg.SetProperty(NHibernate.Cfg.Environment.WrapResultSets, "true");
            cfg.AddDeserializedMapping(GetMappings(), "Main");
            var update = new SchemaUpdate(cfg);
            update.Execute(true, true);
            this.Kernel.Bind<SchemaUpdate>().ToConstant(update).InSingletonScope();

            return cfg.BuildSessionFactory();
        }

        /// <summary>
        /// Generates XML string from <see cref="NHibernate"/> mappings. Used just to verify what was generated by ConfOrm to make sure everything is correct.
        /// </summary>
        protected static string Serialize(HbmMapping hbmElement)
        {
            var setting = new XmlWriterSettings { Indent = true };
            var serializer = new XmlSerializer(typeof(HbmMapping));
            using (var memStream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(memStream, setting))
                {
                    serializer.Serialize(xmlWriter, hbmElement);
                    memStream.Flush();
                    byte[] streamContents = memStream.ToArray();

                    string result = Encoding.UTF8.GetString(streamContents);
                    return result;
                }
            }
        }

        private HbmMapping GetMappings()
        {
            var mapper = new ModelMapper();
            var mappingTypes = this.mappingAssemblies.SelectMany(x => x.GetExportedTypes())
                                                     .Where(x => x.IsSubclassOfRawGeneric(typeof(ClassMapping<>)));
            mapper.AddMappings(mappingTypes);
            mapper.BeforeMapProperty += (inspector, member, customizer) =>
            {
                var propertyInfo = (PropertyInfo) member.LocalMember;
                if (propertyInfo.PropertyType == typeof (string))
                {
                    customizer.Type(NHibernateUtil.StringClob);
                }
            };

            mapper.BeforeMapClass += (inspector, type, customizer) =>
            {
                var tableName = type.Name.Pluralize();
                customizer.Table(tableName);
            };

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }

        private static object GetReadSideRepositoryWriter(IContext context)
        {
            object postgresWriter = context.Kernel.GetService(typeof(PostgreReadSideRepository<>).MakeGenericType(context.GenericArguments[0]));

            //return postgresWriter;

            Type cachedWriterType = typeof(MemoryCachedReadSideRepositoryWriter<>).MakeGenericType(context.GenericArguments[0]);

            object cachingWriter = Activator.CreateInstance(cachedWriterType,
                postgresWriter, new ReadSideStoreMemoryCacheSettings(memoryCacheSizePerEntity, memoryCacheSizePerEntity / 2));

            return cachingWriter;
        }
    }
}