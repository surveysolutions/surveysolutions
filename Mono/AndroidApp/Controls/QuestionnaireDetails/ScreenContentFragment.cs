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
using AndroidApp.Controls.QuestionnaireDetails.ScreenItems;
using AndroidApp.ViewModel.QuestionnaireDetails;
using Main.Core.Entities.SubEntities;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class ScreenContentFragment : Fragment
    {
        public static ScreenContentFragment NewInstance(QuestionnaireScreenViewModel model)
        {
            ScreenContentFragment f = new ScreenContentFragment(model);


            return f;
        }
        public ScreenContentFragment(QuestionnaireScreenViewModel model):base()
        {
            this.Model = model;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no
                // reason to create our view.
                return null;
            }
            ScrollView sv=new ScrollView(inflater.Context);
            sv.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            LinearLayout ll=new LinearLayout(inflater.Context);
            ll.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            ll.Orientation = Orientation.Vertical;
            ll.SetPadding(0, 10, 0, 0);
            
            sv.AddView(ll);
            foreach (var item in Model.Items)
            {
                var question = item as QuestionViewModel;
                View itemView = null;
                if (question != null)
                {

                    switch (question.QuestionType)
                    {
                        case QuestionType.Text:
                            itemView = new TextQuestionView(inflater.Context, question);
                            break;
                        case QuestionType.Numeric:
                            itemView = new NumericQuestionView(inflater.Context, question);
                            break;
                        case QuestionType.DateTime:
                            itemView = new DateQuestionView(inflater.Context, question);
                            break;
                        case QuestionType.SingleOption:
                            itemView = new SingleChoiseQuestionView(inflater.Context, question);
                            break;
                        case QuestionType.MultyOption:
                            itemView = new MultyQuestionView(inflater.Context, question);
                            break;
                        default:
                            itemView = new TextQuestionView(inflater.Context, question);
                            break;
                    }

                }
                var group = item as GroupViewModel;
                if (group != null)
                {
                    itemView = new GroupView(inflater.Context, group);
                }
                if (itemView != null)
                    ll.AddView(itemView);
            }
            return sv;
            /*inflater.Inflate(Resource.Layout.ScreenNavigationView, null);
            this.Container.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(Container_ItemClick);*/
            //  return retval;
        }


        public QuestionnaireScreenViewModel Model { get; private set; }


    }
}