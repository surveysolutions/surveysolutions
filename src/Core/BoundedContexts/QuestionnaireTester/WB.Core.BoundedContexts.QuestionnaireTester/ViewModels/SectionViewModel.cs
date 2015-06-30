using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;

using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    [DebuggerDisplay("Title = {Title}, Id = {SectionIdentity}")]
    public class SectionViewModel : MvxNotifyPropertyChanged
    {
        private readonly SectionsViewModel root;

        public SectionViewModel(SectionsViewModel root)
        {
            this.root = root;
            this.Children = new ObservableCollection<SectionViewModel>();
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

        public ObservableCollection<SectionViewModel> Children { get; set; }

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
            await this.root.NavigateToSection(this);
        }
    }
}