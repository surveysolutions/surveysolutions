using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

namespace AxmlTester.Droid
{
    public class BaseMvxActivity<T> : MvxActivity where T:IMvxViewModel
    {
        public new T ViewModel
        {
            get { return (T) base.ViewModel; }
            set { base.ViewModel = value; }
        }
    }
}