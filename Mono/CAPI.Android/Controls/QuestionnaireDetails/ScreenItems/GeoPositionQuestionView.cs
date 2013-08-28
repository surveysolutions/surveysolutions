using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class GeoPositionQuestionView : AbstractQuestionView
    {
        public GeoPositionQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source, questionnairePublicKey)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            var geoWrapper = new LinearLayout(this.Context);
            geoWrapper.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);

            geoWrapper.Orientation = Orientation.Horizontal;

            locaionDisplay = new TextView(this.Context);
            locaionDisplay.Text = Model.AnswerString;


            var updateLocationButton = new Button(this.Context);
            updateLocationButton.Text = "Get Location";

            geoWrapper.AddView(locaionDisplay);

            geoWrapper.AddView(updateLocationButton);

            llWrapper.AddView(geoWrapper);

        }

        private TextView locaionDisplay;
    }
}