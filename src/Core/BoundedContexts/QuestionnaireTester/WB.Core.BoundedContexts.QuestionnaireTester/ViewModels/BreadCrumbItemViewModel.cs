using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class BreadCrumbItemViewModel
    {
        private readonly NavigationState navigationState;

        public BreadCrumbItemViewModel(NavigationState navigationState)
        {
            this.navigationState = navigationState;
        }

        public string Text { get; set; }
        public Identity ItemId { get; set; }

        public IMvxCommand NavigateCommand
        {
            get
            {
                return new MvxCommand(() =>
                {
                    navigationState.NavigateTo(ItemId);
                });
            }
        }
    }
}