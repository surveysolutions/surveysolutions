using CAPI.Android.Core.Model.ViewModel.Login;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.ViewModels;

namespace CAPI.Android
{
    public class StartApplicationObject
        : MvxApplicationObject
          , IMvxStartNavigation
    {
        public void Start()
        {
           // GenerateEvents(NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus);
                RequestNavigate<LoginViewModel>();
        }

        public bool ApplicationCanOpenBookmarks
        {
            get { return false; }
        }

      
    }
}