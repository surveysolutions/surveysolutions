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
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class SurveyDto : DenormalizerRow
    {
        public SurveyDto(Guid id, string surveyTitle)
        {
            Id = id.ToString();
            SurveyTitle = surveyTitle;
        }

        public SurveyDto()
        {
        }

        public string SurveyTitle { get; private set; }
    }
}