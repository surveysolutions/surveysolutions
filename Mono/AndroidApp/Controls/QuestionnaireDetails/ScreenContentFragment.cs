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
    public class ScreenContentFragment : Fragment
    {
        public static ScreenContentFragment NewInstance(Guid questionnaireId, Guid screenId)
        {
            ScreenContentFragment f = new ScreenContentFragment();

            // Supply index input as an argument.
            Bundle args = new Bundle();
            args.PutString("questionnaireId", questionnaireId.ToString());
            args.PutString("screenId", screenId.ToString());
            f.Arguments = args;

            return f;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no
                // reason to create our view.
                return null;
            }

            TextView tv = new TextView(inflater.Context);
            tv.Text = ScreenId.ToString();
            return tv;
            /*inflater.Inflate(Resource.Layout.ScreenNavigationView, null);
            this.Container.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(Container_ItemClick);*/
            //  return retval;
        }


        public Guid QuestionnaireId
        {
            get { return Guid.Parse(Arguments.GetString("questionnaireId")); }
        }
        public Guid ScreenId
        {
            get { return Guid.Parse(Arguments.GetString("screenId")); }
        }


    }
}