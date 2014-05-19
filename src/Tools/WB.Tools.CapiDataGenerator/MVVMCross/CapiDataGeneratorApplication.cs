using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Tools.CapiDataGenerator.Models;

namespace WB.Tools.CapiDataGenerator.MVVMCross
{
    public class CapiDataGeneratorApplication : MvxApplication
    {
        public CapiDataGeneratorApplication()
        {
            Mvx.RegisterSingleton<IMvxAppStart>(new MvxAppStart<ModeSelectorPageModel>());
        }
    }
}
