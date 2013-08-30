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

            //locationDisplay = CreateTable(Model.AnswerObject as GeoPosition);
            
            locationText = new TextView(this.Context);
            locationText.SetTypeface(null, TypefaceStyle.Bold);
            locationText.Text = RenderPositionAsText(Model.AnswerObject as GeoPosition);

            var updateLocationButton = new Button(this.Context) {Text = "Get Location"};
            var layoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, 
                                                               ViewGroup.LayoutParams.WrapContent);
            layoutParams.AddRule(LayoutRules.AlignParentRight);
            updateLocationButton.LayoutParameters = layoutParams;
            updateLocationButton.Click += GetLocation;

            geoWrapper.AddView(updateLocationButton);
            geoWrapper.AddView(locationText);
            //geoWrapper.AddView(locationDisplay);

            llWrapper.AddView(geoWrapper);
        }

        //private TableLayout locationDisplay;

        private TextView locationText;

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

                    locationText.Text = RenderPositionAsText(position);
                    //locationDisplay = CreateTable(position);
                    
                    SaveAnswer();
                }
            }));
        }

        private TableLayout CreateTable(GeoPosition position)
        {
            var table = new TableLayout(this.Context);


            TableRow th = new TableRow(this.Context);
            TextView columnName = new TextView(this.Context);
            columnName.Text = "Latitude:";
            TextView columnValue = new TextView(this.Context);
            columnValue.Text = position.Latitude.ToString("{0:N4}");

            th.AddView(columnName);
            th.AddView(columnValue);
            table.AddView(th);

             th = new TableRow(this.Context);
             columnName = new TextView(this.Context);
            columnName.Text = "Longitude:";
             columnValue = new TextView(this.Context);
            columnValue.Text = position.Longitude.ToString("{0:N4}");

            th.AddView(columnName);
            th.AddView(columnValue);
            table.AddView(th);

             th = new TableRow(this.Context);
             columnName = new TextView(this.Context);
             columnName.Text = "Accuracy:";
             columnValue = new TextView(this.Context);
             columnValue.Text = position.Accuracy.ToString("{0:N2}");

            th.AddView(columnName);
            th.AddView(columnValue);
            table.AddView(th);

            return table;
        }

        private string RenderPositionAsText(GeoPosition position)
        {
            StringBuilder sb = new StringBuilder();
            if (position != null)
            {
                sb.AppendLine(string.Format("Latitude:\t\t\t{0}{1:N4}", position.Latitude > 0 ? "\t" : "", position.Latitude));
                sb.AppendLine(string.Format("Longitude:\t\t{0}{1:N4}", position.Longitude > 0 ? "\t" : "", position.Longitude));
                sb.AppendLine(string.Format("Accuracy:\t\t{0}{1:N2}", position.Accuracy > 0 ? "\t" : "", position.Accuracy));
            }
            else
            {
                sb.Append("N/A");
            }

            return sb.ToString();
        }

    }
};