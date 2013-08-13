using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Commands.Questionnaire.Completed;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class SingleChoiseQuestionView : AbstractQuestionView
    {
        public SingleChoiseQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source, questionnairePublicKey)
        {
        }

        protected SelectebleQuestionViewModel typedMode;
        protected RadioGroup radioGroup;
        #region Overrides of AbstractQuestionView

        protected override void Initialize()
        {
            base.Initialize();
            typedMode = Model as SelectebleQuestionViewModel;
            RadioButton[]  radioButtons = new RadioButton[typedMode.Answers.Count()];
            radioGroup = new RadioGroup(this.Context);
            radioGroup.Orientation = Orientation.Vertical;
            RadioButton checkedButton = null;
            int i = 0;
            foreach (var answer in typedMode.Answers)
            {
                radioButtons[i] = new RadioButton(this.Context);
                radioButtons[i].Text = answer.Title;
                
                if (answer.Selected)
                    checkedButton = radioButtons[i];
                radioButtons[i].SetTag(Resource.Id.AnswerId, answer.PublicKey.ToString());
                radioButtons[i].AttachImage(answer);
                radioGroup.AddView(radioButtons[i]);
               
                i++;
            }
            if (checkedButton != null)
                radioGroup.Check(checkedButton.Id);
            radioGroup.CheckedChange += radioGroup_CheckedChange;
           
            llWrapper.AddView(radioGroup);
        }
        void radioGroup_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            var selectedItem = radioGroup.FindViewById<RadioButton>(e.CheckedId);
            var answerGuid = Guid.Parse(selectedItem.GetTag(Resource.Id.AnswerId).ToString());

            CommandService.Execute(new AnswerSingleOptionQuestionCommand(this.QuestionnairePublicKey,
                                                                         CapiApplication.Membership.CurrentUser.Id,
                                                                         Model.PublicKey.PublicKey, this.Model.PublicKey.PropagationVector, DateTime.Now,
                                                                             typedMode.Answers.FirstOrDefault(
                                                                                 a => a.PublicKey == answerGuid).Value));
            SaveAnswer();
        }



        #endregion
    }
}