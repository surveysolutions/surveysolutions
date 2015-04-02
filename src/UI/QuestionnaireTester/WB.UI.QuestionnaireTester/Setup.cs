using System.Reflection;
using Android.Content;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
           
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override Assembly[] GetViewModelAssemblies()
        {
            return new[] { typeof(BaseViewModel).Assembly };
        }
    }
}