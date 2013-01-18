using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.Events;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public interface IScreenChanging
    {
         event EventHandler<ScreenChangedEventArgs> ScreenChanged;
    }
}