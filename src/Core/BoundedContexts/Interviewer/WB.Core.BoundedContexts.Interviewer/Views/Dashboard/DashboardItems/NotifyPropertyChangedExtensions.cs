using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MvvmCross.Core.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public static class NotifyPropertyChangedExtensions
    {
        public static void RaiseAndSetIfChanged<TReturn>(this IMvxNotifyPropertyChanged self,
            ref TReturn backingField, 
            TReturn newValue, 
            [CallerMemberName] string propertyName = "",
            params string[] alsoNotify)
        {
            if (EqualityComparer<TReturn>.Default.Equals(backingField, newValue)) return;

            backingField = newValue;

            self.RaisePropertyChanged(propertyName);
            foreach (var notify in alsoNotify)
            {
                self.RaisePropertyChanged(notify);
            }
        }
    }
}