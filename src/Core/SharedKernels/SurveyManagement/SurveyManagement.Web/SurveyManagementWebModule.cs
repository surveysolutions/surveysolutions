using Ninject.Modules;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandDeserialization;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Core.SharedKernels.SurveyManagement.Web
{
    public class SurveyManagementWebModule : NinjectModule
    {
        public override void Load()
        {
            //this.Bind<IUserWebViewFactory>().To<UserWebViewFactory>(); // binded automatically but should not
            this.Bind<ICommandDeserializer>().To<SurveyManagementCommandDeserializer>();
            this.Bind<IRevalidateInterviewsAdministrationService>().To<RevalidateInterviewsAdministrationService>().InSingletonScope();
        }
    }
}