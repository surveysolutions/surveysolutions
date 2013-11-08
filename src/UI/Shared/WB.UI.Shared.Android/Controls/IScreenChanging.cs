using System;
using WB.UI.Shared.Android.Events;

namespace WB.UI.Shared.Android.Controls
{
    public interface IScreenChanging
    {
         event EventHandler<ScreenChangedEventArgs> ScreenChanged;
    }
}