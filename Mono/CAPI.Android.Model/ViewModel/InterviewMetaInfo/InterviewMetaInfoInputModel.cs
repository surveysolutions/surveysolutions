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

namespace CAPI.Android.Core.Model.ViewModel.InterviewMetaInfo
{
    public class InterviewMetaInfoInputModel
    {
        public InterviewMetaInfoInputModel(Guid interviewid)
        {
            InterviewId = interviewid;
        }

        public Guid InterviewId { get; private set; }
    }
}