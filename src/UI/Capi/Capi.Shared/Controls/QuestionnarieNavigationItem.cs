using Android.Content;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.UI.Capi.Shared.Controls
{
    public class QuestionnarieNavigationItem : LinearLayout
    {
        private readonly QuestionnaireScreenViewModel model;
        private readonly View view;
        private readonly TextView tvCount;

        public QuestionnarieNavigationItem(Context context, QuestionnaireScreenViewModel model)
            : base(context)
        {
            this.model = model;

            LayoutInflater layoutInflater = (LayoutInflater) context.GetSystemService(Context.LayoutInflaterService);
            // no view to re-use, create new
            this.view = layoutInflater.Inflate(Resource.Layout.list_navigation_item, null);

            this.AddView(this.view);

            var tvITem = this.view.FindViewById<TextView>(Resource.Id.tvITem);
            tvITem.Text = model.ScreenName;

            this.tvCount = this.view.FindViewById<TextView>(Resource.Id.tvCount);
            
            this.UpdateCounters();

            model.PropertyChanged += this.model_PropertyChanged;
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Answered" && e.PropertyName != "Total")
                return;

            this.UpdateCounters();
        }

        private void UpdateCounters()
        {
            this.tvCount.Text = string.Format("{0}/{1}", this.model.Answered, this.model.Total);
            if (this.model.Total == this.model.Answered)
                this.tvCount.SetBackgroundResource(Resource.Drawable.donecountershape);
            else
                this.tvCount.SetBackgroundResource(Resource.Drawable.CounterRoundShape);
        }

    }
}