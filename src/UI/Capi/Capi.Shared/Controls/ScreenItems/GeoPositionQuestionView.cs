using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.UI.Capi.Shared.GeolocationServices;
using Xamarin.Geolocation;

namespace WB.UI.Capi.Shared.Controls.ScreenItems
{
    public class GeoPositionQuestionView : AbstractQuestionView
    {
        public GeoPositionQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
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
            
            this.locationText = new TextView(this.Context);
            
            this.locationText.SetTypeface(null, TypefaceStyle.Bold);
            this.PutAnswerStoredInModelToUI();

            var updateLocationButton = new Button(this.Context) {Text = "Get Location"};
            var layoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, 
                                                               ViewGroup.LayoutParams.WrapContent);
            layoutParams.AddRule(LayoutRules.AlignParentRight);
            updateLocationButton.LayoutParameters = layoutParams;
            updateLocationButton.Click += this.GetLocation;

            geoWrapper.AddView(updateLocationButton);
            geoWrapper.AddView(this.locationText);

            this.llWrapper.AddView(geoWrapper);
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            var position = this.Model.AnswerObject as GeoPosition;

            return position != null
                ? this.RenderPositionAsText(position.Latitude, position.Longitude, position.Accuracy)
                : string.Empty;
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.locationText.Text = this.GetAnswerStoredInModelAsString();
        }

        private TextView locationText;

        private void GetLocation(object sender, EventArgs e)
        {
            if (this.geoservice == null)
                this.geoservice = new GeoService(this.Context);

            if (!this.geoservice.IsGeolocationAvailable || !this.geoservice.IsGeolocationEnabled)
            {
                Toast.MakeText(this.Context, "Geo location is unavailable", ToastLength.Long).Show();
                return;
            }

            this.progress = ProgressDialog.Show(this.Context, "Determining location", "Please Wait...", true, true);
            this.progress.CancelEvent += this.progressCoordinates_Cancel;

            Task.Factory.StartNew(this.GetLocation);
        }

        private void progressCoordinates_Cancel(object sender, EventArgs e)
        {
            CancellationTokenSource cancel = this.cancelSource;
            if (cancel != null)
                cancel.Cancel();
        }

        private void GetLocation()
        {
            this.cancelSource = new CancellationTokenSource();

            var activity = this.Context as Activity;
            if (activity == null)
                return;

            this.geoservice.GetPositionAsync(20000, this.cancelSource.Token).ContinueWith((Task<Position>t) => activity.RunOnUiThread(() =>
            {
                if (this.progress != null)
                    this.progress.Dismiss();

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
                    var positionAsText = this.RenderPositionAsText(t.Result.Latitude, t.Result.Longitude, t.Result.Accuracy);

                    this.locationText.Text = positionAsText;

                    this.SaveAnswer(
                        positionAsText,
                        new AnswerGeoLocationQuestionCommand(
                            this.QuestionnairePublicKey, this.Membership.CurrentUser.Id, this.Model.PublicKey.Id,
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