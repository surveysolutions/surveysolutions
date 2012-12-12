using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class ScreenContentView : LinearLayout
    {
        public ScreenContentView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {

            Initialize(context);
        }

        public ScreenContentView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize(context);
        }

        private void Initialize(Context context)
        {
            this.ScreensContainer=new Dictionary<Guid, View>();
            
           /* LayoutInflater layoutInflater = (LayoutInflater) context.GetSystemService(Context.LayoutInflaterService);
            layoutInflater.Inflate(Resource.Layout.ScreenGroupView, this);*/
        }


        public Guid QuestionnaireId { get; set; }
        public void ShowScreen(Guid newScreenId)
        {
           /* TextView qGuid = FindViewById<TextView>(Resource.Id.qGuid);
            qGuid.Text = screenId.ToString();*/
            if (ScreensContainer.ContainsKey(ScreenId))
            {
                ScreensContainer[ScreenId].Visibility = ViewStates.Gone;
            }
            if (ScreensContainer.ContainsKey(newScreenId))
            {
                ScreensContainer[newScreenId].Visibility = ViewStates.Visible;
            }
            else
            {

                TextView tv = new TextView(this.Context);
                tv.Text = newScreenId.ToString();
                ScreensContainer.Add(newScreenId, tv);
                this.AddView(tv);

            }
            this.ScreenId = newScreenId;
        }

        public Guid ScreenId { get;private set; }
        protected IDictionary<Guid, View> ScreensContainer;

    }
}