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
            
            sv.AddView(ll);
            foreach (var question in Model.Items)
            {
                AbstractQuestionView questionView;
                switch (question.QuestionType)
                {
                    case QuestionType.Text:
                        questionView = new TextQuestionView(inflater.Context, question);
                        break;
                    case QuestionType.Numeric:
                        questionView = new NumericQuestionView(inflater.Context, question);
                        break;
                    case QuestionType.DateTime:
                        questionView = new DateQuestionView(inflater.Context, question);
                        break;
                    case QuestionType.SingleOption:
                        questionView = new SingleChoiseQuestionView(inflater.Context, question);
                        break;
                    case QuestionType.MultyOption:
                        questionView = new MultyQuestionView(inflater.Context, question);
                        break;
                    default:
                        questionView = new TextQuestionView(inflater.Context, question);
                        break;
                }
                ll.AddView(questionView);
            }
            return sv;
            /*inflater.Inflate(Resource.Layout.ScreenNavigationView, null);
            this.Container.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(Container_ItemClick);*/
            //  return retval;
        }


        public QuestionnaireScreenViewModel Model { get; private set; }


    }
}