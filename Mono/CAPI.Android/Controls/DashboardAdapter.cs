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
using CAPI.Android.Core.Model.ViewModel.Dashboard;

namespace CAPI.Android.Controls
{
    public class DashboardAdapter : BaseAdapter<DashboardSurveyItem>
    {
        private  DashboardModel _dashboard;
        private View[] items;
        private readonly Context context;
        public DashboardAdapter(Context context, Guid userId)
        {
            _dashboard = CapiApplication.LoadView<DashboardInput, DashboardModel>(
                new DashboardInput(userId));
            this.context = context;
            this.items=new View[_dashboard.Surveys.Count];
        }

        public void Update()
        {
            this.items = new View[_dashboard.Surveys.Count];
            _dashboard = CapiApplication.LoadView<DashboardInput, DashboardModel>(
               new DashboardInput(_dashboard.OwnerKey));
            NotifyDataSetChanged();
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = items[position];
            if (view == null)
            {
                LayoutInflater layoutInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                // no view to re-use, create new
                view = layoutInflater.Inflate(Resource.Layout.dashboard_survey_row, null);



                var txtSurveyName = view.FindViewById<TextView>(Resource.Id.txtSurveyName);
                txtSurveyName.Text = _dashboard.Surveys[position].SurveyTitle;
                var txtSurveyCount = view.FindViewById<TextView>(Resource.Id.txtSurveyCount);
                txtSurveyCount.Text = _dashboard.Surveys[position].ActiveItems.Count.ToString();


                var llQuestionnarieHolder = view.FindViewById<LinearLayout>(Resource.Id.llQuestionnarieHolder);
                foreach (var qustionnarieItem in _dashboard.Surveys[position].ActiveItems)
                {
                    // no view to re-use, create new
                    var questionnairieview = layoutInflater.Inflate(Resource.Layout.dashboard_survey_item, null);
                    llQuestionnarieHolder.AddView(questionnairieview);
                    var llQuestionnairie =
                        questionnairieview.FindViewById<LinearLayout>(Resource.Id.llQuestionnairie);
                    llQuestionnairie.Click += (s, e) =>
                        {
                            var intent = new Intent(context, typeof (LoadingActivity));
                            intent.PutExtra("publicKey", qustionnarieItem.PublicKey.ToString());
                            context.StartActivity(intent);
                        };
                    var tvStatus = questionnairieview.FindViewById<TextView>(Resource.Id.tvStatus);
                    tvStatus.Text = qustionnarieItem.Status.Name;

                    var llPropertyHolder = questionnairieview.FindViewById<LinearLayout>(Resource.Id.llPropertyHolder);
                    foreach (var featuredItem in qustionnarieItem.Properties)
                    {
                        var propertyView = layoutInflater.Inflate(Resource.Layout.dashboard_survey_property_item, null);
                        llPropertyHolder.AddView(propertyView);
                        var txtPropertyName = propertyView.FindViewById<TextView>(Resource.Id.txtPropertyName);
                        txtPropertyName.Text = featuredItem.Title;
                        var txtPropertyValue = propertyView.FindViewById<TextView>(Resource.Id.txtPropertyValue);
                        txtPropertyValue.Text = featuredItem.Value;
                    }
                }
                /* if (position < Count - 1)
                {
                    var item = model.Chapters[position];
                    var closure = new UpdateTotalClosure(tvCount, item);
                    closure.UpdateCounter();
                    this.subscribers[position] = closure;
                    tvITem.Text = item.ScreenName;

                    view.SetTag(Resource.Id.ScreenId, item.ScreenId.ToString());
                }
                else
                {
                    tvITem.Text = SurveyStatus.IsStatusAllowCapiSync(model.Status) ? "Summary" : "Complete";
                    tvCount.Visibility = ViewStates.Gone;
                }*/
                items[position] = view;
            }
            return view;
        }

        void questionnairieview_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public override int Count
        {
            get { return _dashboard.Surveys.Count; }
        }

        public override DashboardSurveyItem this[int position]
        {
            get { return _dashboard.Surveys[position]; }
        }
    }
}