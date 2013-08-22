using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class QuestionnaireNavigationAdapter : BaseAdapter<QuestionnaireScreenViewModel>
    {
        private readonly CompleteQuestionnaireView model;
        private readonly Context context;
        private readonly int selectedItem;
        private readonly UpdateTotalClosure[] subscribers;

        public QuestionnaireNavigationAdapter(Context context, CompleteQuestionnaireView model, int selectedItem){
            this.context = context;
            this.selectedItem = selectedItem;
            this.model = model;
            this.subscribers=new UpdateTotalClosure[this.model.Chapters.Count];
        }


        #region Overrides of BaseAdapter

        public override long GetItemId(int position)
        {
            return position;
        }
        
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView != null)
                return convertView;

            
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

        public override int Count
        {
            get { return model.Chapters.Count + 1; }
        }

        #endregion

        #region Overrides of BaseAdapter<QuestionnaireNavigationPanelItem>

        public override QuestionnaireScreenViewModel this[int position]
        {
            get
            {
                return model.Chapters[position];
            }
        }

        #endregion

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