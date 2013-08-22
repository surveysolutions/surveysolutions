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
    public class DashboardAdapter : BaseAdapter<DashboardQuestionnaireItem>
    {
        private IList<DashboardQuestionnaireItem> questionnaries;
        private readonly Activity activity;
        public DashboardAdapter(Activity activity, IList<DashboardQuestionnaireItem> questionnaries)
        {
            this.questionnaries = questionnaries;
            this.activity = activity;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView != null)
                return convertView;
            var dataItem = this[position];
            LayoutInflater layoutInflater =
                (LayoutInflater) activity.GetSystemService(Context.LayoutInflaterService);

            View view = layoutInflater.Inflate(Resource.Layout.dashboard_survey_item, null);

            var llQuestionnairie =
                view.FindViewById<LinearLayout>(Resource.Id.llQuestionnairie);
            view.SetTag(Resource.Id.QuestionnaireId, dataItem.PublicKey.ToString());
            llQuestionnairie.Focusable = false;
            var tvStatus = view.FindViewById<TextView>(Resource.Id.tvStatus);
            tvStatus.Text = dataItem.Status.Name;

            var llPropertyHolder = view.FindViewById<LinearLayout>(Resource.Id.llPropertyHolder);
            foreach (var featuredItem in dataItem.Properties)
            {
                var propertyView = layoutInflater.Inflate(Resource.Layout.dashboard_survey_property_item, null);
                llPropertyHolder.AddView(propertyView);
                var txtPropertyName = propertyView.FindViewById<TextView>(Resource.Id.txtPropertyName);
                txtPropertyName.Text = featuredItem.Title;
                var txtPropertyValue = propertyView.FindViewById<TextView>(Resource.Id.txtPropertyValue);
                txtPropertyValue.Text = featuredItem.Value;
            }

            return view;
        }


        public override int Count
        {
            get { return questionnaries.Count; }
        }

        public override DashboardQuestionnaireItem this[int position]
        {
            get { return questionnaries[position]; }
        }
    }
}