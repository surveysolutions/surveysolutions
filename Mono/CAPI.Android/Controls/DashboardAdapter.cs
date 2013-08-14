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
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Controls
{
    public class DashboardAdapter : BaseAdapter<DashboardQuestionnaireItem>
    {
        private IList<DashboardQuestionnaireItem> questionnaries;
        private IDictionary<Guid, View> items;
        private readonly Activity activity;
        public DashboardAdapter(Activity activity, IList<DashboardQuestionnaireItem> questionnaries)
        {
            this.questionnaries = questionnaries;
            this.activity = activity;
            this.items = new Dictionary<Guid, View>();
        }

     /*   public void Update()
        {
            this.items = new View[_dashboard.Surveys.Count];
            _dashboard = CapiApplication.LoadView<DashboardInput, DashboardModel>(
               new DashboardInput(_dashboard.OwnerKey));
            activity.RunOnUiThread(() => NotifyDataSetChanged());
        
        }*/

        public void Update(IList<DashboardQuestionnaireItem> newQuestionnaries)
        {
            var unhandledQuestionnariesKey = newQuestionnaries.Select(q=>q.PublicKey).ToList();
            var isUpdated = false;
            foreach (var questionnarie in questionnaries)
            {
                var newQuestionnarie = newQuestionnaries.FirstOrDefault(q => q.PublicKey == questionnarie.PublicKey);
                if (newQuestionnarie == null)
                {
                    items.Remove(questionnarie.PublicKey);
                    isUpdated = true;
                    continue;
                }
                unhandledQuestionnariesKey.Remove(newQuestionnarie.PublicKey);
                if (newQuestionnarie.Status != questionnarie.Status)
                {
                    items.Remove(questionnarie.PublicKey);
                    isUpdated = true;
                }
            }
            if (unhandledQuestionnariesKey.Count > 0)
            {
                isUpdated = true;
            }
            questionnaries = newQuestionnaries;
            if (isUpdated)
                activity.RunOnUiThread(() => { this.NotifyDataSetChanged(); });
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var dataItem = this[position];
            if (!items.ContainsKey(dataItem.PublicKey))
            {
                LayoutInflater layoutInflater =
                    (LayoutInflater) activity.GetSystemService(Context.LayoutInflaterService);

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

                items.Add(dataItem.PublicKey, view);
            }
            return items[dataItem.PublicKey];
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