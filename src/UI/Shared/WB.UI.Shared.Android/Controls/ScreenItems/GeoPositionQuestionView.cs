using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.UI.Shared.Android.GeolocationServices;
using Xamarin.Geolocation;

namespace WB.UI.Shared.Android.Controls.ScreenItems
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


            this.locationText = new TextView(this.CurrentContext);
            this.locationText.SetTypeface(null, TypefaceStyle.Bold);

            this.InitializeViewAndButtonView(this.locationText, "Get Location", this.GetLocation);

            this.PutAnswerStoredInModelToUI();
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            var position = this.Model.AnswerObject as GeoPosition;

            return position != null
                ? this.RenderPositionAsText(position.Latitude, position.Longitude, position.Accuracy, position.Altitude)
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
                this.geoservice = new GeoService(this.CurrentContext);

            if (!this.geoservice.IsGeolocationAvailable || !this.geoservice.IsGeolocationEnabled)
            {
                Toast.MakeText(this.CurrentContext, "Geo location is unavailable", ToastLength.Long).Show();
                return;
            }

            this.progress = ProgressDialog.Show(this.CurrentContext, "Determining location", "Please Wait...", true, true);
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

            var activity = this.CurrentContext as Activity;
            if (activity == null)
                return;

            this.geoservice.GetPositionAsync(300000, this.cancelSource.Token).ContinueWith((Task<Position>t) => activity.RunOnUiThread(() =>
            {
                if (this.progress != null)
                    this.progress.Dismiss();

                if (t.IsCanceled)
                {
                    Toast.MakeText(this.CurrentContext, "Canceled or Timeout.", ToastLength.Long).Show();
                }
                else if (t.IsFaulted)
                {
                    Toast.MakeText(this.CurrentContext, "Error occurred on location retrieving.", ToastLength.Long).Show();
                }
                else
                {
                    var positionAsText = this.RenderPositionAsText(t.Result.Latitude, t.Result.Longitude, t.Result.Accuracy, t.Result.Altitude);

                    this.locationText.Text = positionAsText;

                    this.SaveAnswer(
                        positionAsText,
                        new AnswerGeoLocationQuestionCommand(
                            this.QuestionnairePublicKey, this.Membership.CurrentUser.Id, this.Model.PublicKey.Id,
                            this.Model.PublicKey.InterviewItemPropagationVector, DateTime.UtcNow, t.Result.Latitude, t.Result.Longitude,
                            t.Result.Accuracy, t.Result.Altitude, t.Result.Timestamp));
                }
            }));
        }

        private string RenderPositionAsText(double latitude, double longitude, double accuracy, double altitude)
        {
            StringBuilder sb = new StringBuilder();
            
                sb.AppendLine(string.Format("Latitude:\u0009\u0009\u0009{0}{1:N4}", latitude > 0 ? " " : "", latitude));
                sb.AppendLine(string.Format("Longitude:\u0009\u0009{0}{1:N4}", longitude > 0 ? " " : "", longitude));
                sb.AppendLine(string.Format("Accuracy:\u0009\u0009 {0:N2}m", accuracy));
                sb.AppendLine(string.Format("Altitude:\u0009\u0009 {0:N2}m", altitude));
            
            return sb.ToString();
        }
    }
};