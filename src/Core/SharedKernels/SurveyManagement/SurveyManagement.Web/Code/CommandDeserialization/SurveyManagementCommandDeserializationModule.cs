using Ninject.Modules;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandDeserialization
{
    public class SurveyManagementCommandDeserializationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ICommandDeserializer>().To<SurveyManagementCommandDeserializer>();
        }
    }
}
