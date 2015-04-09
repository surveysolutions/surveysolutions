using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class ApplicationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<JsonUtilsSettings>().ToConstant(new JsonUtilsSettings() {TypeNameHandling = TypeSerializationSettings.None});
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>().InSingletonScope();
            this.Bind<ApplicationSettings>().ToSelf().InSingletonScope();
        }
    }
}