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
            LayoutInflater layoutInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);

            View view;

            if (position < Count - 1)
            {
                view = new QuestionnarieNavigationItem(context, dataItem);
                view.SetTag(Resource.Id.ScreenId, dataItem.ScreenId.ToString());
            }
            else
            {
                view = layoutInflater.Inflate(Resource.Layout.list_navigation_item, null);

               

                var tvITem = view.FindViewById<TextView>(Resource.Id.tvITem);
                var tvCount = view.FindViewById<TextView>(Resource.Id.tvCount);

                tvITem.Text = status == InterviewStatus.Completed ? "Summary" : "Complete";
                tvCount.Visibility = ViewStates.Gone;
            }

            if (position == selectedItem)
            {
                view.SetBackgroundColor(Color.LightBlue);
            }

            return view;
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