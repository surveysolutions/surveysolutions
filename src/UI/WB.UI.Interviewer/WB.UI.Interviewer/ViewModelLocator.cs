using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using WB.UI.Interviewer.ViewModel;

namespace WB.UI.Interviewer
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<InterviewViewModel>();
        }

        public InterviewViewModel InterviewDetails
        {
            get
            {
                return ServiceLocator.Current.GetInstance<InterviewViewModel>();
            }
        }
    }
}
