namespace WB.UI.Designer.App_Start
{
    using Ncqrs.Commanding.ServiceModel;
    using Ninject.Modules;

    /// <summary>
    /// The main module.
    /// </summary>
    public class MainModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ICommandService>().ToConstant(Ncqrs.NcqrsEnvironment.Get<ICommandService>());
        }
    }
}
