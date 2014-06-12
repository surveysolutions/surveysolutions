using Ninject.Modules;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Headquarters.Code.CommandDeserialization
{
    public class SurveyManagementCommandDeserializationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ICommandDeserializer>().To<SurveyManagementCommandDeserializer>();
        }
    }
}
