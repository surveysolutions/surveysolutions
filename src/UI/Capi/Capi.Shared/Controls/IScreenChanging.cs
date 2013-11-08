using System;
using WB.UI.Capi.Shared.Events;

namespace WB.UI.Capi.Shared.Controls
{
    public interface IScreenChanging
    {
         event EventHandler<ScreenChangedEventArgs> ScreenChanged;
    }
}