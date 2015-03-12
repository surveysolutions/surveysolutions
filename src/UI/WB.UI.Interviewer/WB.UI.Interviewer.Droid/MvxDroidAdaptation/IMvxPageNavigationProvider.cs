using Xamarin.Forms;

namespace WB.UI.Interviewer.Droid.MvxDroidAdaptation
{
    public interface IMvxPageNavigationProvider
    {
        void Push(Page page);
        void Pop();
    }
}