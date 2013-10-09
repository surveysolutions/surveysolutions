using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.GeolocationServices;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using Xamarin.Geolocation;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class GeoPositionQuestionView : AbstractQuestionView
    {
        public GeoPositionQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService)
        {
        }

        private IGeoService geoservice;
        private ProgressDialog progress;
        private CancellationTokenSource cancelSource;

        protected override void Initialize()
        {
            base.Initialize();

            var geoWrapper = new RelativeLayout(this.Context);
            geoWrapper.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);

            //geoWrapper.Orientation = Orientation.Horizontal;
            
            locationText = new TextView(this.Context);
            
            locationText.SetTypeface(null, TypefaceStyle.Bold);
            this.PutAnswerStoredInModelToUI();

            var updateLocationButton = new Button(this.Context) {Text = "Get Location"};
            var layoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, 
                                                               ViewGroup.LayoutParams.WrapContent);
            layoutParams.AddRule(LayoutRules.AlignParentRight);
            updateLocationButton.LayoutParameters = layoutParams;
            updateLocationButton.Click += GetLocation;

            geoWrapper.AddView(updateLocationButton);
            geoWrapper.AddView(locationText);

            llWrapper.AddView(geoWrapper);
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            var position = Model.AnswerObject as GeoPosition;

            if (position != null)
            {
                locationText.Text = RenderPositionAsText(position.Latitude, position.Longitude, position.Accuracy);
            }
        }

        private TextView locationText;

        private void GetLocation(object sender, EventArgs e)
        {
            if (geoservice == null)
                geoservice = new GeoService(this.Context);

            if (!this.geoservice.IsGeolocationAvailable || !this.geoservice.IsGeolocationEnabled)
            {
                Toast.MakeText(this.Context, "Geo location is unavailable", ToastLength.Long).Show();
                return;
            }

            progress = ProgressDialog.Show(this.Context, "Determining location", "Please Wait...", true, true);
            progress.CancelEvent += progressCoordinates_Cancel;

            Task.Factory.StartNew(GetLocation);
        }

        private void progressCoordinates_Cancel(object sender, EventArgs e)
        {
            CancellationTokenSource cancel = this.cancelSource;
            if (cancel != null)
                cancel.Cancel();
        }

        private void GetLocation()
        {
            cancelSource = new CancellationTokenSource();

            var activity = this.Context as Activity;
            if (activity == null)
                return;

            geoservice.GetPositionAsync(20000, cancelSource.Token).ContinueWith((Task<Position>t) => activity.RunOnUiThread(() =>
            {
                if (progress != null)
                    progress.Dismiss();

                if (t.IsCanceled)
                {
                    Toast.MakeText(this.Context, "Canceled or Timeout.", ToastLength.Long).Show();
                }
                else if (t.IsFaulted)
                {
                    Toast.MakeText(this.Context, "Error occurred on location retrieving.", ToastLength.Long).Show();
                }
                else
                {
                    var positionAsText = RenderPositionAsText(t.Result.Latitude, t.Result.Longitude, t.Result.Accuracy);

                    locationText.Text = positionAsText;

                    this.SaveAnswer(
                        positionAsText,
                        new AnswerGeoLocationQuestionCommand(
                            this.QuestionnairePublicKey, CapiApplication.Membership.CurrentUser.Id, Model.PublicKey.Id,
                            this.Model.PublicKey.PropagationVector, DateTime.UtcNow, t.Result.Latitude, t.Result.Longitude,
                            t.Result.Accuracy, t.Result.Timestamp));
                }
            }));
        }

        private string RenderPositionAsText(double latitude, double longitude, double accuracy)
        {
            StringBuilder sb = new StringBuilder();
            
                sb.AppendLine(string.Format("Latitude:\u0009\u0009\u0009{0}{1:N4}", latitude > 0 ? " " : "", latitude));
                sb.AppendLine(string.Format("Longitude:\u0009\u0009{0}{1:N4}", longitude > 0 ? " " : "", longitude));
                sb.AppendLine(string.Format("Accuracy:\u0009\u0009 {0:N2}m", accuracy));
            
            return sb.ToString();
        }
    }
};