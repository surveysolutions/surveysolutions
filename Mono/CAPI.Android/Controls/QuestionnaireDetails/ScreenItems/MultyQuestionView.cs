using System;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Main.Core.Commands.Questionnaire.Completed;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class MultyQuestionView : AbstractQuestionView
    {
      /*  public MultyQuestionView(Context context, QuestionViewModel model)
            : base(context, model)
        {
        }
        */
        public MultyQuestionView(Context context, IMvxBindingActivity bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source, questionnairePublicKey)
        {
        }

        /*public MultyQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionViewModel model)
            : base(context, attrs, defStyle, model)
        {
        }

        public MultyQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionViewModel model)
            : base(javaReference, transfer, model)
        {
        }*/

        #region Overrides of AbstractQuestionView

        protected override void Initialize()
        {
            base.Initialize();
            this.Orientation = Orientation.Horizontal;

            typedMode = Model as SelectebleQuestionViewModel;
            var rl = new RelativeLayout(this.Context);
            rl.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
           
            int i = 1;
            foreach (var answer in typedMode.Answers)
            {
                CheckBox cb = new CheckBox(this.Context);
                cb.Id = i;
                var layoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                                   ViewGroup.LayoutParams.WrapContent);
                if (i>1)
                {
                    layoutParams.AddRule(LayoutRules.AlignTop, i-1);
                    layoutParams.AddRule(LayoutRules.RightOf, i-1);
                }
                else
                    layoutParams.AddRule(LayoutRules.AlignLeft);
                cb.LayoutParameters = layoutParams;
                cb.Text = answer.Title;
                cb.Checked = answer.Selected;
                cb.CheckedChange += cb_CheckedChange;
                cb.SetTag(Resource.Id.AnswerId, answer.PublicKey.ToString());

                cb.AttachImage(answer);
                rl.AddView(cb);
                i++;
            }
            llWrapper.AddView(rl);
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