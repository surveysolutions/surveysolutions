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
        private View[] items;
        public QuestionnaireNavigationAdapter(Context context, CompleteQuestionnaireView model, int selectedItem)
            : base()
        {
            this.context = context;
            this.selectedItem = selectedItem;
            this.model = model;
            this.items = new View[this.Count];
        }


        #region Overrides of BaseAdapter

        public override long GetItemId(int position)
        {
            return position;
        }
        
        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            View view = items[position];
            if (view == null)
            {
                LayoutInflater layoutInflater = (LayoutInflater) context.GetSystemService(Context.LayoutInflaterService);
                // no view to re-use, create new
                view = layoutInflater.Inflate(Resource.Layout.list_navigation_item, null);

                if (position == selectedItem)
                {
                    view.SetBackgroundColor(Color.LightBlue);
                }

                var tvITem = view.FindViewById<TextView>(Resource.Id.tvITem);

                var tvCount = view.FindViewById<TextView>(Resource.Id.tvCount);
                if (position < Count - 1)
                {
                    var item = model.Chapters[position];
                    item.PropertyChanged += (sender,e)=>
                    {
                        if (e.PropertyName != "Answered" && e.PropertyName != "Total")
                            return;
                        var question = sender as QuestionnaireScreenViewModel;
                        if (question == null)
                            return;
                        UpdateCounter(tvCount, item);
                    }
                    ;
                    tvITem.Text = item.ScreenName;
                    UpdateCounter(tvCount, item);
                    view.SetTag(Resource.Id.ScreenId, item.ScreenId.ToString());
                }
                else
                {
                    tvITem.Text = SurveyStatus.IsStatusAllowCapiSync(model.Status) ? "Summary" : "Complete";
                    tvCount.Visibility = ViewStates.Gone;
                }
                items[position] = view;
            }
            return view;
        }
        protected void UpdateCounter(TextView tvCount, QuestionnaireScreenViewModel item)
        {
            tvCount.Text = string.Format("{0}/{1}", item.Answered, item.Total);
            if(item.Total==item.Answered)
                tvCount.SetBackgroundResource(Resource.Drawable.donecountershape);
            else
                tvCount.SetBackgroundResource(Resource.Drawable.CounterRoundShape);
        }

        public void SelectItem(int pos)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                    continue;
                items[i].SetBackgroundColor(i == pos ? Color.LightBlue : Color.Transparent);
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
    }
}