using System.Reflection;
using Ninject;
using Ninject.Activation;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Core.Infrastructure.Storage.Raven
{
    public class RavenReadSideInfrastructureModule : RavenInfrastructureModule
    {
        private readonly Assembly[] assembliesWithIndexes;
        private readonly RavenReadSideRepositoryWriterSettings ravenReadSideRepositoryWriterSettings;

        public RavenReadSideInfrastructureModule(RavenConnectionSettings settings,RavenReadSideRepositoryWriterSettings ravenReadSideRepositoryWriterSettings, params Assembly[] assembliesWithIndexes)
            : base(settings)
        {
            this.assembliesWithIndexes = assembliesWithIndexes;
            this.ravenReadSideRepositoryWriterSettings = ravenReadSideRepositoryWriterSettings;
        }

        public override void Load()
        {
            this.BindDocumentStore();

            this.Bind<IReadSideRepositoryIndexAccessor>().To<RavenReadSideRepositoryIndexAccessor>().InSingletonScope()
                .WithConstructorArgument("assembliesWithIndexes", this.assembliesWithIndexes);

            this.Bind<ReadSideService>().ToSelf().InSingletonScope();
            this.Bind<IReadSideStatusService>().ToMethod(context => this.Kernel.Get<ReadSideService>());
            this.Bind<IReadSideAdministrationService>().ToMethod(context => this.Kernel.Get<ReadSideService>());
           
            this.Bind<RavenReadSideRepositoryWriterSettings>().ToConstant(ravenReadSideRepositoryWriterSettings);

            // each repository writer should exist in one instance because it might use caching
            this.Kernel.Bind(typeof(RavenReadSideRepositoryWriter<>)).ToSelf().InSingletonScope();

            this.Kernel.Bind(typeof(IReadSideRepositoryReader<>)).ToMethod(this.GetReadSideRepositoryReader);
            this.Kernel.Bind(typeof(IQueryableReadSideRepositoryReader<>)).ToMethod(this.GetReadSideRepositoryReader);
            this.Kernel.Bind(typeof(IReadSideRepositoryWriter<>)).ToMethod(this.GetReadSideRepositoryWriter);


          //  this.Kernel.Bind(typeof(FilesStoreRepositoryAccessor<>)).ToSelf().InSingletonScope();
            //this.Kernel.Bind(typeof(RavenFilesStoreRepositoryAccessor<>)).ToSelf().InSingletonScope();
            //this.Kernel.Bind(typeof(IReadSideKeyValueStorage<>)).ToMethod(this.GetKeyValueStorage);

        }

        protected object GetReadSideRepositoryWriter(IContext context)
        {
            return this.Kernel.Get(typeof(RavenReadSideRepositoryWriter<>).MakeGenericType(context.GenericArguments[0]));
        }

        protected object GetReadSideRepositoryReader(IContext context)
        {
            return this.Kernel.Get(typeof(RavenReadSideRepositoryReader<>).MakeGenericType(context.GenericArguments[0]));
        }
    }
}