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

            LayoutInflater layoutInflater = (LayoutInflater) context.GetSystemService(Context.LayoutInflaterService);
            layoutInflater.Inflate(Resource.Layout.ScreenGroupView, this);
        }


        public Guid QuestionnaireId { get; set; }

        public Guid? ScreenId
        {
            get { return screenId; }
            set
            {
                screenId = value;
                TextView qGuid = FindViewById<TextView>(Resource.Id.qGuid);
                qGuid.Text = value.ToString();
            }
        }

        private Guid? screenId;
    }
}