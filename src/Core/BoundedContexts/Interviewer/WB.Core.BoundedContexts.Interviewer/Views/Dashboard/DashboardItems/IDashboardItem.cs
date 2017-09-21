using System;
using MvvmCross.Core.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public interface IDashboardItem
    {
        bool HasExpandedView { get; }
        bool IsExpanded { get; set; }
    }

    public interface IDashboardViewItem
    {
        string Title { get; }
        string SubTitle { get; }
        string IdLabel { get; }

        string MainActionLabel { get; }
        IMvxAsyncCommand MainAction { get; }
        bool MainActionEnabled { get;  }

        bool HasAdditionalActions { get; }
        MenuAction[] Actions { get; }
    }

    public class MenuAction
    {
        public MenuAction(string label, IMvxCommand action, bool enabled)
        {
            this.Label = label;
            this.Action = action;
            this.Enabled = enabled;
        }

        public string Label { get; set; }
        public IMvxCommand Action { get; set; }
        public Boolean Enabled { get; set; }
        public int MenuItemId { get; set; }
    }
}