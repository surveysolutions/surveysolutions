using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
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
        public GeoPositionQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source, questionnairePublicKey)
        {
        }

        private IGeoService geoservice;
        private ProgressDialog progress;
        private CancellationTokenSource cancelSource;

        protected override void Initialize()
        {
            base.Initialize();

            var geoWrapper = new LinearLayout(this.Context);
            geoWrapper.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);

            geoWrapper.Orientation = Orientation.Vertical;

            locaionDisplay = new TextView(this.Context);
            locaionDisplay.Text = Model.AnswerString;
            
            var updateLocationButton = new Button(this.Context) {Text = "Get Location"};
            updateLocationButton.Click += GetLocation;
            updateLocationButton.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, 
                ViewGroup.LayoutParams.WrapContent);
            
            geoWrapper.AddView(updateLocationButton);
            geoWrapper.AddView(locaionDisplay);

            llWrapper.AddView(geoWrapper);
        }

        private TextView locaionDisplay;

        private void GetLocation(object sender, EventArgs e)
        {
            if (geoservice == null)
                geoservice = new GeoService(this.Context);

            if (!this.geoservice.IsGeolocationAvailable || !this.geoservice.IsGeolocationEnabled)
            {
                Toast.MakeText(this.Context, "Geolocation is unavailable", ToastLength.Long).Show();
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
                    Toast.MakeText(this.Context, "Error occured on location retrieving.", ToastLength.Long).Show();
                }
                else
                {
                    var position = new GeoPosition()
                        {
                            Latitude = t.Result.Latitude,
                            Longitude = t.Result.Longitude,
                            Accuracy = t.Result.Accuracy,
                            Timestamp = t.Result.Timestamp
                        };
                    
                    
                    CommandService.Execute(new AnswerGeoLocationQuestionCommand(this.QuestionnairePublicKey, CapiApplication.Membership.CurrentUser.Id, Model.PublicKey.PublicKey,
                                                                  this.Model.PublicKey.PropagationVector, DateTime.UtcNow, position));
                    locaionDisplay.Text = position.ToString();
                    
                    SaveAnswer();
                }
            }));
        }
    }
}