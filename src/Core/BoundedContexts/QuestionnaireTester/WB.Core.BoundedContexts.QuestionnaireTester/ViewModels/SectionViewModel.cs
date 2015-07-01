using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
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
            this.Children.CollectionChanged += (sender, args) => this.RaisePropertyChanged(() => HasChildren);
            this.Expanded = true;
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

        public bool HasChildren
        {
            get { return this.Children.Count > 0; }
        }

        public bool Expanded
        {
            get { return this.expanded; }
            set
            {
                if (this.Expanded != value)
                {
                    this.expanded = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<SectionViewModel> Children { get; private set; }

        private MvxCommand navigateToSectionCommand;
        private bool expanded;
        private string toggleButtonText;

        public ICommand NavigateToSectionCommand
        {
            get
            {
                this.navigateToSectionCommand = this.navigateToSectionCommand ?? new MvxCommand(async () => await this.NavigateToSection());
                return this.navigateToSectionCommand;
            }
        }

        public ICommand Toggle
        {
            get
            {
                return new MvxCommand(() => this.Expanded = !this.Expanded, () => HasChildren);
            }
        }

        private async Task NavigateToSection()
        {
            await this.root.NavigateToSection(this);
        }
    }
}