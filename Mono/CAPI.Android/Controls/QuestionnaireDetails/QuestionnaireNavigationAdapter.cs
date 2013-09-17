using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class QuestionnaireNavigationAdapter : SmartAdapter<QuestionnaireScreenViewModel>
    {
        private readonly Context context;
        private readonly int selectedItem;
        private readonly InterviewStatus status;

        public QuestionnaireNavigationAdapter(Context context, CompleteQuestionnaireView model, int selectedItem)
            : base(model.Chapters)
        {
            this.context = context;
            this.selectedItem = selectedItem;
            this.status = model.Status;
        }

        protected override View BuildViewItem(QuestionnaireScreenViewModel dataItem, int position)
        {

            LayoutInflater layoutInflater = (LayoutInflater) context.GetSystemService(Context.LayoutInflaterService);
            // no view to re-use, create new
            var view = layoutInflater.Inflate(Resource.Layout.list_navigation_item, null);

            if (position == selectedItem)
            {
                view.SetBackgroundColor(Color.LightBlue);
            }

            var tvITem = view.FindViewById<TextView>(Resource.Id.tvITem);

            var tvCount = view.FindViewById<TextView>(Resource.Id.tvCount);
            if (position < Count - 1)
            {
                dataItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName != "Answered" && e.PropertyName != "Total")
                        return;

                    UpdateCounters(dataItem, tvCount);
                };
                tvITem.Text = dataItem.ScreenName;
                UpdateCounters(dataItem, tvCount);
                view.SetTag(Resource.Id.ScreenId, dataItem.ScreenId.ToString());
            }
            else
            {
                tvITem.Text = status == InterviewStatus.Completed ? "Summary" : "Complete";
                tvCount.Visibility = ViewStates.Gone;
            }
            return view;
        }

        private static void UpdateCounters(QuestionnaireScreenViewModel dataItem, TextView tvCount)
        {
            tvCount.Text = string.Format("{0}/{1}", dataItem.Answered, dataItem.Total);
            if (dataItem.Total == dataItem.Answered)
                tvCount.SetBackgroundResource(Resource.Drawable.donecountershape);
            else
                tvCount.SetBackgroundResource(Resource.Drawable.CounterRoundShape);
        }

        public override QuestionnaireScreenViewModel this[int position]
        {
            get
            {
                if (position == Count - 1)
                    return null;
                return base[position];
            }
        }

        public override int Count
        {
            get { return base.Count + 1; }
        }
    }
}