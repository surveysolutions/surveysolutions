using Ninject.Modules;
using WB.UI.Shared.Web.CommandDeserialization;

namespace Web.Supervisor.CommandDeserialization
{
    public class SupervisorCommandDeserializationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ICommandDeserializer>().To<SupervisorCommandDeserializer>();
        }
    }
}
