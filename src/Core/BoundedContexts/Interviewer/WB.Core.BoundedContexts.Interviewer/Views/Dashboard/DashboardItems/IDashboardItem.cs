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

        IMvxCommand OpenMenu { get; }
        bool HasAdditionalActions { get; }
    }
}