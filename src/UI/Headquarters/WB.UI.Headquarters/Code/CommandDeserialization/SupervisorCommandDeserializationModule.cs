using Ninject.Modules;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Headquarters.Code.CommandDeserialization
{
    public class SupervisorCommandDeserializationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ICommandDeserializer>().To<HeadquartersCommandDeserializer>();
        }
    }
}
