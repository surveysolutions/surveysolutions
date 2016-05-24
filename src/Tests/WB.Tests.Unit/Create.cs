using WB.Tests.Unit.TestFactories;

namespace WB.Tests.Unit
{
    internal class Create
    {
        public static readonly ServiceFactory Service = new ServiceFactory(); // cleaned by TLK (remove this comment after KP-7207 is done)
        public static readonly ControllerFactory Controller = new ControllerFactory();

        public static readonly CommandFactory Command = new CommandFactory(); // cleaned by TLK (remove this comment after KP-7207 is done)
        public static readonly EventFactory Event = new EventFactory(); // cleaned by TLK (remove this comment after KP-7207 is done)
        public static readonly PublishedEventFactory PublishedEvent = new PublishedEventFactory(); // cleaned by TLK (remove this comment after KP-7207 is done)

        public static readonly ViewModelFactory ViewModel = new ViewModelFactory();
        public static readonly AnswerFactory Answer = new AnswerFactory();

        public static readonly OtherFactory Other = new OtherFactory();
    }
}
