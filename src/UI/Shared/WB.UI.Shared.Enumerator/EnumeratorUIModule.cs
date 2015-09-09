using Ninject.Modules;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Enumerator
{
    public class EnumeratorUIModule : NinjectModule
    {
        public override void Load()
        {
           // this.Bind<IFragmentTypeLookup>().To<FragmentTypeLookup>();
        }
    }
}