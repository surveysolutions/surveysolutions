using WB.Tests.Unit.TestFactories;

namespace WB.Tests.Unit
{
    internal class Create
    {
        public static readonly EntityFactory Entity = new EntityFactory();

        public static readonly ServiceFactory Service = new ServiceFactory();
        public static readonly ControllerFactory Controller = new ControllerFactory();
        public static readonly ViewModelFactory ViewModel = new ViewModelFactory();

        public static readonly CommandFactory Command = new CommandFactory();
        public static readonly EventFactory Event = new EventFactory();
        public static readonly PublishedEventFactory PublishedEvent = new PublishedEventFactory();

        public static readonly OtherFactory Other = new OtherFactory();
    }
}
