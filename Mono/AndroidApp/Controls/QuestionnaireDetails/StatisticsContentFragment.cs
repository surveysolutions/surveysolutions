using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class StatisticsContentFragment : Fragment
    {

        public static StatisticsContentFragment NewInstance(Guid questionnaireKey)
        {
            StatisticsContentFragment f = new StatisticsContentFragment(questionnaireKey);


            return f;
        }

        public Guid QuestionnaireKey { get; private set; }

        public StatisticsContentFragment(Guid questionnaireKey)
            : base()
        {
            this.QuestionnaireKey = questionnaireKey;
        }
        public override void OnCreate(Bundle p0)
        {
            base.OnCreate(p0);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no
                // reason to create our view.
                return null;
            }
            var view = inflater.Inflate(Resource.Layout.StatisticsContent, null);
            return view;
            /*inflater.Inflate(Resource.Layout.ScreenNavigationView, null);
            this.Container.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(Container_ItemClick);*/
            //  return retval;
        }
    }
}