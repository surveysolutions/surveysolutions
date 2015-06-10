using Android.Views;

namespace WB.UI.QuestionnaireTester.CustomControls.MvxBindableAutoCompleteTextViewControl
{
    public interface IMvxBindingActivity
    {
        void ClearBindings(View view);
        View BindingInflate(object source, int resourceId, ViewGroup viewGroup);
        View BindingInflate(int resourceId, ViewGroup viewGroup);
        View NonBindingInflate(int resourceId, ViewGroup viewGroup);
    }
}