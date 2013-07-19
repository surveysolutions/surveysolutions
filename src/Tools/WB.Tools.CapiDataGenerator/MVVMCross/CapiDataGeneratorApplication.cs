using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

namespace CapiDataGenerator
{
    public class CapiDataGeneratorApplication : MvxApplication
    {
        public CapiDataGeneratorApplication()
        {
            Mvx.RegisterSingleton<IMvxAppStart>(new MvxAppStart<MainPageModel>());
        }
    }
}
