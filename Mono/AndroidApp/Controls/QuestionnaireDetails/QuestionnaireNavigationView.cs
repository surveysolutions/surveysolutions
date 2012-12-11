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
using AndroidApp.ViewModel.QuestionnaireDetails;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class QuestionnaireNavigationView : ListView
    {
        public Guid QuestionnaireId
        {
            get { return questionnaireId; }
            set
            {
                questionnaireId = value;
               /* this.RemoveAllViews();*/
                var questionnaireData =
                    CapiApplication.LoadView<QuestionnaireNavigationPanelInput, QuestionnaireNavigationPanelModel>(
                        new QuestionnaireNavigationPanelInput(this.QuestionnaireId));
                this.Adapter = new ArrayAdapter<string>(this.Context, Resource.Layout.list_navigation_item,
                                                        questionnaireData.Items.Select(i => i.Title).ToArray());
            }
        }

        private Guid questionnaireId;
        public QuestionnaireNavigationView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize(attrs);
        }

        public QuestionnaireNavigationView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize(attrs);
        }

        private void Initialize(IAttributeSet attrs)
        {


        }
    }
}