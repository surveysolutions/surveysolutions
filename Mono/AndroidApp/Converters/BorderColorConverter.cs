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
using AndroidApp.ViewModel.QuestionnaireDetails;

namespace AndroidApp.Converters
{
    public class BorderColorConverter : Cirrious.MvvmCross.Converters.MvxBaseValueConverter
    {

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var status = (QuestionStatus) value;
            if (!/*Model.Enabled*/status.HasFlag(QuestionStatus.Enabled))
                return -1;
            else
            {
                if (!/*Model.Valid*/status.HasFlag(QuestionStatus.Valid))
                    return Resource.Drawable.questionInvalidShape;
                else if (/*Model.Answered*/status.HasFlag(QuestionStatus.Answered))
                    return Resource.Drawable.questionAnsweredShape;
            }
            return Resource.Drawable.questionShape;
        }
    }
}