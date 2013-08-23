using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class QuestionnaireNavigationAdapter : SmartAdapter<QuestionnaireScreenViewModel>
    {
        private readonly Context context;
        private readonly int selectedItem;
        private readonly UpdateTotalClosure[] subscribers;
        private readonly SurveyStatus status;

        public QuestionnaireNavigationAdapter(Context context, CompleteQuestionnaireView model, int selectedItem)
            : base(model.Chapters)
        {
            this.context = context;
            this.selectedItem = selectedItem;
            this.status = model.Status;
            this.subscribers=new UpdateTotalClosure[model.Chapters.Count];
        }

        protected override View BuildViewItem(QuestionnaireScreenViewModel dataItem, int position)
        {

            LayoutInflater layoutInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
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
                var closure = new UpdateTotalClosure(tvCount, dataItem);
                closure.UpdateCounter();
                this.subscribers[position] = closure;
                tvITem.Text = dataItem.ScreenName;

                view.SetTag(Resource.Id.ScreenId, dataItem.ScreenId.ToString());
            }
            else
            {
                tvITem.Text = SurveyStatus.IsStatusAllowCapiSync(status) ? "Summary" : "Complete";
                tvCount.Visibility = ViewStates.Gone;
            }
            return view;
        }

        public void Detach()
        {
            foreach (var subscriber in subscribers)
            {
                if(subscriber==null)
                    continue;
                subscriber.Detach();
            }
        }

        public void Attach()
        {
            foreach (var subscriber in subscribers)
            {
                if (subscriber == null)
                    continue;
                subscriber.Attach();
            }
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

        public class  UpdateTotalClosure
        {
            private readonly TextView tvCount;
            private readonly QuestionnaireScreenViewModel model;

            public UpdateTotalClosure(TextView tvCount, QuestionnaireScreenViewModel model)
            {
                this.tvCount = tvCount;
                this.model = model;
                Attach();
            }

            protected void TotalChangedHandler(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName != "Answered" && e.PropertyName != "Total")
                    return;

                UpdateCounter();
            }
            public void UpdateCounter()
            {
                tvCount.Text = string.Format("{0}/{1}", model.Answered, model.Total);
                if (model.Total == model.Answered)
                    tvCount.SetBackgroundResource(Resource.Drawable.donecountershape);
                else
                    tvCount.SetBackgroundResource(Resource.Drawable.CounterRoundShape);
            }

            public void Attach()
            {
                this.model.PropertyChanged += TotalChangedHandler;
            }

            public void Detach()
            {
                this.model.PropertyChanged -= TotalChangedHandler;
            }
        }
    }
}