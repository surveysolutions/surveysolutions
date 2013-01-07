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
using Cirrious.MvvmCross.Converters;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Localization.Interfaces;
namespace AndroidApp.Converters
{
    public class AnsweredTotalConverter : MvxBaseValueConverter
          , IMvxServiceConsumer<IMvxTextProvider>
    {
    }
}