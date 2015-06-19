using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;

using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SectionViewModel : MvxNotifyPropertyChanged
    {
        private readonly SectionsViewModel parent;

        public SectionViewModel(SectionsViewModel parent)
        {
            this.parent = parent;
        }

        public Identity SectionIdentity { get; set; }

        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set { this.isSelected = value; this.RaisePropertyChanged(); }
        }

        private MvxCommand navigateToSectionCommand;
        public System.Windows.Input.ICommand NavigateToSectionCommand
        {
            get
            {
                this.navigateToSectionCommand = this.navigateToSectionCommand ?? new MvxCommand(async () => await this.NavigateToSection());
                return this.navigateToSectionCommand;
            }
        }

        private async Task NavigateToSection()
        {
            await this.parent.NavigateToSection(this);
        }
    }
}