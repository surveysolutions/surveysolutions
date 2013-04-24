namespace WB.UI.Designer.App_Start
{
    using Ncqrs.Commanding.ServiceModel;
    using Ninject.Modules;
    using Ninject.Web.Mvc.FilterBindingSyntax;


    /// <summary>
    /// The main module.
    /// </summary>
    public class MainModule : NinjectModule
    {
        public override void Load()
        {
            this.BindFilter<CustomAuthorizeFilter>(System.Web.Mvc.FilterScope.Controller, 0).WhenControllerHas<CustomAuthorizeAttribute>().InSingletonScope();
            this.Bind<ICommandService>().ToConstant(Ncqrs.NcqrsEnvironment.Get<ICommandService>());
        }
    }
}
