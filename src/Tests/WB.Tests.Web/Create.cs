using WB.Tests.Web.TestFactories;

namespace WB.Tests.Web
{
    internal class Create
    {
        public static readonly AggregateRootFactory AggregateRoot = new AggregateRootFactory();
        public static readonly EntityFactory Entity = new EntityFactory();

        public static readonly ServiceFactory Service = new ServiceFactory();

        public static readonly CommandFactory Command = new CommandFactory();

        public static readonly StorageFactory Storage = new StorageFactory();

        public static readonly ControllerFactory Controller = new ControllerFactory();

    }
}
