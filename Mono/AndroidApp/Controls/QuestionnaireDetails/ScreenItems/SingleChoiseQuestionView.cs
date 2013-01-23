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
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Main.Core.Commands.Questionnaire.Completed;

namespace AndroidApp.Controls.QuestionnaireDetails.ScreenItems
{
    public class SingleChoiseQuestionView : AbstractQuestionView
    {
       /* public SingleChoiseQuestionView(Context context, QuestionViewModel model) : base(context, model)
        {
        }*/

        public SingleChoiseQuestionView(Context context, IMvxBindingActivity bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source, questionnairePublicKey)
        {
        }

      /*  public SingleChoiseQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionViewModel model) : base(context, attrs, defStyle, model)
        {
        }

        public SingleChoiseQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionViewModel model) : base(javaReference, transfer, model)
        {
        }*/
        protected SelectebleQuestionViewModel typedMode;
        protected RadioGroup radioGroup;
        #region Overrides of AbstractQuestionView

        protected override void Initialize()
        {
            base.Initialize();
            typedMode = Model as SelectebleQuestionViewModel;
            var radioButton = new RadioButton[typedMode.Answers.Count()];
            radioGroup = new RadioGroup(this.Context);
            radioGroup.Orientation = Orientation.Vertical;
            //radioGroup.
            int i = 0;
            foreach (var answer in typedMode.Answers)
            {
                radioButton[i] = new RadioButton(this.Context);
                
                radioGroup.AddView(radioButton[i]);
                radioButton[i].Text = answer.Title;
                radioButton[i].Checked = answer.Selected;
                radioButton[i].SetTag(Resource.Id.AnswerId, answer.PublicKey.ToString());

            }
            radioGroup.CheckedChange += radioGroup_CheckedChange;
            llWrapper.AddView(radioGroup);
        }

        void radioGroup_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            var selectedItem = radioGroup.FindViewById<RadioButton>(e.CheckedId);
            var answerGuid = Guid.Parse(selectedItem.GetTag(Resource.Id.AnswerId).ToString());

            CommandService.Execute(new SetAnswerCommand(this.QuestionnairePublicKey, Model.PublicKey.PublicKey,
                                                         new List<Guid>(1) {answerGuid}, "",
                                                         Model.PublicKey.PropagationKey));
            //typedMode.SelectAnswer(answerGuid);
        }



        #endregion
    }
}