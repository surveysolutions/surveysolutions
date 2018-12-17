﻿using Android.App;
using Android.Views;
using WB.Core.BoundedContexts.Interviewer.Views.CreateInterview;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "",
        Theme = "@style/GrayAppTheme",
        WindowSoftInputMode = SoftInput.StateHidden,
        NoHistory = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
        Exported = false)]
    public class CreateAndLoadInterviewActivity : ProgressInterviewActivity<CreateAndLoadInterviewViewModel>
    {
        public override bool IsSupportMenu => false;
    }
}
