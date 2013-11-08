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
using CAPI.Android.Core;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Capi.Shared.Adapters;

namespace CAPI.Android.Controls
{
    public class DashboardAdapter : SmartAdapter<DashboardQuestionnaireItem>
    {
        private readonly Activity activity;

        public DashboardAdapter(IList<DashboardQuestionnaireItem> items, Activity activity) : base(items)
        {
            this.activity = activity;
        }

        protected override View BuildViewItem(DashboardQuestionnaireItem dataItem, int position)
        {
            LayoutInflater layoutInflater =
               (LayoutInflater)activity.GetSystemService(Context.LayoutInflaterService);

            View view = layoutInflater.Inflate(Resource.Layout.dashboard_survey_item, null);

            var llQuestionnairie =
                view.FindViewById<LinearLayout>(Resource.Id.llQuestionnairie);
            view.SetTag(Resource.Id.QuestionnaireId, dataItem.PublicKey.ToString());
            llQuestionnairie.Focusable = false;
            var tvStatus = view.FindViewById<TextView>(Resource.Id.tvStatus);
                tvStatus.Text = dataItem.Status.ToString();

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
    }
}