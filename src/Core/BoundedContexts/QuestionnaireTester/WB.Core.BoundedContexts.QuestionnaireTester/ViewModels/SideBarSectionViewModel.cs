using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    [DebuggerDisplay("Title = {Title}, Id = {SectionIdentity}")]
    public class SideBarSectionViewModel : MvxNotifyPropertyChanged
    {
        private readonly SideBarSectionsViewModel root;
        private readonly SideBarSectionViewModel parent;

        public SideBarSectionViewModel(SideBarSectionsViewModel root, SideBarSectionViewModel parent, int nodeDepth)
        {
            this.NodeDepth = nodeDepth;
            this.root = root;
            this.parent = parent;
            this.Children = new ObservableCollection<SideBarSectionViewModel>();
            this.Children.CollectionChanged += (sender, args) => this.RaisePropertyChanged(() => HasChildren);
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

        public bool IsCurrent
        {
            get { return this.isCurrent; }
            set { this.isCurrent = value; this.RaisePropertyChanged(); }
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

        public int NodeDepth { get; set; }

        public ObservableCollection<SideBarSectionViewModel> Children { get; private set; }

        private MvxCommand navigateToSectionCommand;
        private bool expanded;
        private bool isCurrent;

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
                return new MvxCommand(() => this.Expanded = !this.Expanded);
            }
        }

        public SideBarSectionViewModel Parent
        {
            get { return parent; }
        }

        private async Task NavigateToSection()
        {
            await this.root.NavigateToSection(this);
        }
    }
}