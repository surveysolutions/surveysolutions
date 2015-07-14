using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class BreadCrumbItemViewModel : MvxNotifyPropertyChanged
    {
        private readonly NavigationState navigationState;
        private string text;

        public BreadCrumbItemViewModel(NavigationState navigationState)
        {
            this.navigationState = navigationState;
        }

        public string Text
        {
            get { return this.text; }
            set { this.text = value; this.RaisePropertyChanged(); }
        }

        public Identity ItemId { get; set; }

        public IMvxCommand NavigateCommand
        {
            get
            {
                return new MvxCommand(async () => await this.navigationState.NavigateTo(this.ItemId));
            }
        }
    }
}