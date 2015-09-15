//using System;
//using System.Collections.Generic;
//using Android.App;
//using Android.Content;
//using Android.Views;
//using Android.Widget;
//using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
//using WB.UI.Interviewer.Controls.Adapters;
//using WB.UI.Interviewer.ViewModel.Dashboard;
//
//namespace WB.UI.Interviewer.Controls
//{
//    public class DashboardAdapter : SmartAdapter<DashboardQuestionnaireItem>
//    {
//        private readonly Activity activity;
//
//        private readonly Action<Guid, View> deleteHandler;
//
//        public DashboardAdapter(IList<DashboardQuestionnaireItem> items, Activity activity, Action<Guid, View> deleteHandler)
//            : base(items)
//        {
//            this.activity = activity;
//            this.deleteHandler = deleteHandler;
//        }
//
//        protected override View BuildViewItem(DashboardQuestionnaireItem dataItem, int position)
//        {
//            View view = this.activity.LayoutInflater.Inflate(Resource.Layout.dashboard_survey_item, null);
//
//            var llQuestionnaire = view.FindViewById<LinearLayout>(Resource.Id.llQuestionnairie);
//            view.SetTag(Resource.Id.QuestionnaireId, dataItem.PublicKey.ToString());
//            llQuestionnaire.Focusable = false;
//
//            Button delButton = view.FindViewById<Button>(Resource.Id.btnRemoveInterview);
//
//            delButton.Visibility = dataItem.CanBeDeleted ? ViewStates.Visible : ViewStates.Gone;
//            delButton.Click += (sender, args) =>
//            {
//                var target = sender as Button;
//                if (target == null)
//                    return;
//
//                this.deleteHandler.Invoke(dataItem.PublicKey, view);
//            };
//
//            view.SetTag(Resource.Id.IsInterviewLocal, dataItem.CreatedOnClient);
//
//
//            TextView tvInterviewStatus = view.FindViewById<TextView>(Resource.Id.txtInterviewStatus);
//            tvInterviewStatus.Text = this.GetStatusText(dataItem.Status);
//          
//            foreach (var featuredItem in dataItem.Properties)
//            {
//                var answerValue = featuredItem.Value;
//                this.AddPropertyToContainer(llQuestionnaire, featuredItem.Title + ": " + answerValue);
//            }
//            
//            var tvArrow = new TextView(this.activity);
//            var img = this.activity.Resources.GetDrawable(global::Android.Resource.Drawable.IcMediaPlay);
//            tvArrow.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);
//            llQuestionnaire.AddView(tvArrow, new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WrapContent, 1));
//
//            var tvComment = view.FindViewById<TextView>(Resource.Id.tvComment);
//
//            if (string.IsNullOrEmpty(dataItem.Comments))
//                tvComment.Visibility = ViewStates.Gone;
//            else
//                tvComment.Text = dataItem.Comments;
//            return view;
//        }
//        
//        protected string GetStatusText(InterviewStatus status)
//        {
//            switch (status)
//            {
//                case InterviewStatus.InterviewerAssigned:
//                    return "Initial";
//                case InterviewStatus.Completed:
//                    return "Completed";
//                case InterviewStatus.Restarted:
//                    return "Restarted";
//                case InterviewStatus.RejectedBySupervisor:
//                    return "Rejected";
//                default:
//                    return "Unknown";
//            }
//        }
//
//        protected TextView AddPropertyToContainer(ViewGroup container, string text)
//        {
//            LayoutInflater layoutInflater =
//                (LayoutInflater) this.activity.GetSystemService(Context.LayoutInflaterService);
//            var propertyView = layoutInflater.Inflate(Resource.Layout.dashboard_survey_property_item, null) as TextView;
//            propertyView.Text = text;
//            container.AddView(propertyView, new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WrapContent, 1));
//            return propertyView;
//        }
//    }
//}