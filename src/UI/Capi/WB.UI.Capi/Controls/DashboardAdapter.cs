using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Adapters;

namespace WB.UI.Capi.Controls
{
    public class DashboardAdapter : SmartAdapter<DashboardQuestionnaireItem>
    {
        private readonly Activity activity;

        private readonly Action<Guid, View> deleteHandler;

        public DashboardAdapter(IList<DashboardQuestionnaireItem> items, Activity activity, Action<Guid, View> deleteHandler)
            : base(items)
        {
            this.activity = activity;
            this.deleteHandler = deleteHandler;
        }

        protected override View BuildViewItem(DashboardQuestionnaireItem dataItem, int position)
        {
            LayoutInflater layoutInflater =
               (LayoutInflater)this.activity.GetSystemService(Context.LayoutInflaterService);

            View view = layoutInflater.Inflate(Resource.Layout.dashboard_survey_item, null);

            var llQuestionnairie =
                view.FindViewById<LinearLayout>(Resource.Id.llQuestionnairie);
            view.SetTag(Resource.Id.QuestionnaireId, dataItem.PublicKey.ToString());
            llQuestionnairie.Focusable = false;

            if (dataItem.CreatedOnClient.HasValue && dataItem.CreatedOnClient.Value)
            {
                Button delButton = new Button(activity);
                delButton.SetTypeface(null, TypefaceStyle.Bold);
                delButton.Text = "-";
                delButton.SetBackgroundColor(Color.Red);
                llQuestionnairie.AddView(delButton, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                                                  ViewGroup.LayoutParams.WrapContent));

                delButton.Click += (sender, args) =>
                {
                    var target = sender as Button;
                    if(target == null)
                        return;

                    this.deleteHandler.Invoke(dataItem.PublicKey, target.Parent as View);
                };

                view.SetTag(Resource.Id.IsInterviewLocal, true);
            }

            AddPropertyToContainer(llQuestionnairie, GetStatusText(dataItem.Status));
          
            foreach (var featuredItem in dataItem.Properties)
            {
                AddPropertyToContainer(llQuestionnairie, featuredItem.Title + ": " + featuredItem.Value);
            }
            
            var tvArrow = new TextView(activity);
            var img = activity.Resources.GetDrawable(global::Android.Resource.Drawable.IcMediaPlay);
            tvArrow.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);
            llQuestionnairie.AddView(tvArrow, new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WrapContent, 1));
            return view;
        }
        
        protected string GetStatusText(InterviewStatus status)
        {
            switch (status)
            {
                case InterviewStatus.InterviewerAssigned:
                    return "Initial";
                    break;
                case InterviewStatus.Completed:
                    return "Completed";
                    break;
                case InterviewStatus.Restarted:
                    return "Restarted";
                    break;
                case InterviewStatus.RejectedBySupervisor:
                    return "Rejected";
                    break;
                default:
                    return "Unknown";
            }
        }

        protected TextView AddPropertyToContainer(ViewGroup container, string text)
        {
            LayoutInflater layoutInflater =
                (LayoutInflater) this.activity.GetSystemService(Context.LayoutInflaterService);
            var propertyView = layoutInflater.Inflate(Resource.Layout.dashboard_survey_property_item, null) as TextView;
            propertyView.Text = text;
            container.AddView(propertyView, new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WrapContent, 1));
            return propertyView;
        }
    }
}