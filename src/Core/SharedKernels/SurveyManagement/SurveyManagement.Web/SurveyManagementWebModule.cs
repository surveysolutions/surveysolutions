using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandDeserialization;
using WB.Core.SharedKernels.SurveyManagement.Web.Implementation.Services;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Core.SharedKernels.SurveyManagement.Web
{
    public class SurveyManagementWebModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ICommandDeserializer>().To<SurveyManagementCommandDeserializer>();
            this.Bind<IRevalidateInterviewsAdministrationService>().To<RevalidateInterviewsAdministrationService>().InSingletonScope();
            this.Bind<ILocalizationService>().To<LocalizationService>().InSingletonScope();
        }
    }
}