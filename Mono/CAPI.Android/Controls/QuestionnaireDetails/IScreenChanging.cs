using System;
using CAPI.Android.Events;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public interface IScreenChanging
    {
         event EventHandler<ScreenChangedEventArgs> ScreenChanged;
    }
}