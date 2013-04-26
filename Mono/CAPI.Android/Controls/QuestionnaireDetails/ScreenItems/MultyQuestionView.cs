using System;
using System.Linq;
using Android.App;
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
       /*   */
           
            int i = 0;
            var checkboxes = new CheckBox[typedMode.Answers.Count()];
            foreach (var answer in typedMode.Answers)
            {
                CheckBox cb = new CheckBox(this.Context);
                cb.Text = answer.Title;
                cb.Checked = answer.Selected;
                cb.CheckedChange += cb_CheckedChange;
                cb.SetTag(Resource.Id.AnswerId, answer.PublicKey.ToString());

                cb.AttachImage(answer);
                checkboxes[i] = cb;
                i++;
            }
            var optionsWrapper = new LinearLayout(this.Context);
            optionsWrapper.Orientation=Orientation.Vertical;
            optionsWrapper.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            PopulateComboboxes(optionsWrapper, checkboxes, this.Context);
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

        private void PopulateComboboxes(LinearLayout ll, CheckBox[] views, Context mContext)
        {
            Display display = ((Activity)mContext).WindowManager.DefaultDisplay;
            ll.RemoveAllViews();
            int maxWidth = display.Width - 20;

            LinearLayout.LayoutParams lparams;
            LinearLayout newLL = new LinearLayout(mContext);
            newLL.LayoutParameters = new LayoutParams(LayoutParams.FillParent,
                                                      LayoutParams.WrapContent);
            newLL.SetGravity(GravityFlags.Left);
            newLL.Orientation = Orientation.Horizontal;

            int widthSoFar = 0;

            for (int i = 0; i < views.Length; i++)
            {
                LinearLayout LL = new LinearLayout(mContext);
                LL.Orientation = Orientation.Horizontal;
                LL.SetGravity(GravityFlags.CenterHorizontal | GravityFlags.Bottom);
                LL.LayoutParameters = new ListView.LayoutParams(
                    LayoutParams.WrapContent, LayoutParams.WrapContent);
                views[i].Measure(0, 0);
                lparams = new LinearLayout.LayoutParams(views[i].MeasuredWidth,
                                                        LayoutParams.WrapContent);
                LL.AddView(views[i], lparams);
                LL.Measure(0, 0);
                widthSoFar += views[i].MeasuredWidth; // YOU MAY NEED TO ADD THE MARGINS
                if (widthSoFar >= maxWidth)
                {
                    ll.AddView(newLL);

                    newLL = new LinearLayout(mContext);
                    newLL.LayoutParameters = new LayoutParams(
                        LayoutParams.FillParent,
                        LayoutParams.WrapContent);
                    newLL.Orientation = Orientation.Horizontal;
                    newLL.SetGravity(GravityFlags.Left);
                    lparams = new LinearLayout.LayoutParams(LL.MeasuredWidth, LL.MeasuredHeight);
                    newLL.AddView(LL, lparams);
                    widthSoFar = LL.MeasuredWidth;
                }
                else
                {
                    newLL.AddView(LL);
                }
            }
            ll.AddView(newLL);
        }
    }
}