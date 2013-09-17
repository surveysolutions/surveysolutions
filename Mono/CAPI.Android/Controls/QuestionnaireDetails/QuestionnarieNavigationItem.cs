using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;

namespace CAPI.Android.Controls.QuestionnaireDetails
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
            view = layoutInflater.Inflate(Resource.Layout.list_navigation_item, null);

            this.AddView(view);

            var tvITem = view.FindViewById<TextView>(Resource.Id.tvITem);
            tvITem.Text = model.ScreenName;

            tvCount = view.FindViewById<TextView>(Resource.Id.tvCount);
            
            this.UpdateCounters();

            model.PropertyChanged += model_PropertyChanged;
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