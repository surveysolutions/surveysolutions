using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Commands.Questionnaire.Completed;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class MultyQuestionView : AbstractQuestionView
    {
        public MultyQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source, questionnairePublicKey)
        {
        }

        #region Overrides of AbstractQuestionView

        protected override void Initialize()
        {
            base.Initialize();
            this.Orientation = Orientation.Vertical;

            typedMode = Model as SelectebleQuestionViewModel;
            var optionsWrapper = new LinearLayout(this.Context);
            optionsWrapper.Orientation = Orientation.Vertical;
            optionsWrapper.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            foreach (var answer in typedMode.Answers)
            {
                CheckBox cb = new CheckBox(this.Context);
                cb.Text = answer.Title;
                cb.Checked = answer.Selected;
                cb.CheckedChange += cb_CheckedChange;
                cb.SetTag(Resource.Id.AnswerId, answer.PublicKey.ToString());

                cb.AttachImage(answer);
                optionsWrapper.AddView(cb);
            }
            llWrapper.AddView(optionsWrapper);
        }

        private SelectebleQuestionViewModel typedMode;
        #endregion

        void cb_CheckedChange(object sender, CheckBox.CheckedChangeEventArgs e)
        {
            var cb = sender as CheckBox;
            var answerGuid = Guid.Parse(cb.GetTag(Resource.Id.AnswerId).ToString());
            var answered = typedMode.Answers.Where(a => a.Selected).Select(a => a.PublicKey).ToList();
            if(e.IsChecked)
                answered.Add(answerGuid);
            else
            {
                answered.Remove(answerGuid);
            }
            CommandService.Execute(new SetAnswerCommand(this.QuestionnairePublicKey, Model.PublicKey.PublicKey,
                                                         answered, "",
                                                         Model.PublicKey.PropagationKey));
            SaveAnswer();

        }
    }
}