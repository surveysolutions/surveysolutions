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
            var position = Model.AnswerObject as GeoPosition;
            if (position!= null)
                locationText.Text = RenderPositionAsText(position.Latitude, position.Longitude, position.Accuracy);

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
                    CommandService.Execute(new AnswerGeoLocationQuestionCommand(this.QuestionnairePublicKey, CapiApplication.Membership.CurrentUser.Id,
                        Model.PublicKey.Id, this.Model.PublicKey.PropagationVector, DateTime.UtcNow, 
                        t.Result.Latitude, t.Result.Longitude, t.Result.Accuracy, t.Result.Timestamp));

                    var positionAsText = RenderPositionAsText(t.Result.Latitude, t.Result.Longitude, t.Result.Accuracy);

                    locationText.Text = positionAsText;
                    //locationDisplay = CreateTable(position);

                    SaveAnswer(positionAsText);
                }
            }));
        }

        /*private TableLayout CreateTable(GeoPosition position)
        {
            var table = new TableLayout(this.Context);

            if (position != null)
            {
                CreateAndAddRow(table, "Latitude:", position.Latitude);
                CreateAndAddRow(table, "Longitude:", position.Longitude);
                CreateAndAddRow(table, "Accuracy:", position.Accuracy);
            }
            else
            {
                var nonValueRow = new TableRow(this.Context);
                var nonValueText = new TextView(this.Context) {Text = "N/A"};
                nonValueText.SetTypeface(null, TypefaceStyle.Bold);
                nonValueRow.AddView(nonValueText);
                table.AddView(nonValueRow);
            }

            return table;
        }

        private void CreateAndAddRow(TableLayout table, string name, double value)
        {
            TableRow th = new TableRow(this.Context);
            TextView columnName = new TextView(this.Context);
            columnName.Text = name;
            columnName.SetTypeface(null, TypefaceStyle.Bold);

            TextView columnValue = new TextView(this.Context);

            var textToShow = value.ToString("N4");
            
            //placeholder for minus sign
            if (value >= 0)
                textToShow = " " + textToShow;
            
            columnValue.SetTypeface(null, TypefaceStyle.Bold);
            columnValue.Text = textToShow;

            th.AddView(columnName);
            th.AddView(columnValue);
            table.AddView(th);
        }*/

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