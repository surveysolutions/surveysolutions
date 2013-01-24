using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Main.Core.Commands.Questionnaire.Completed;

namespace AndroidApp.Controls.QuestionnaireDetails.ScreenItems
{
    public class NumericQuestionView : AbstractQuestionView
    {
       /* public NumericQuestionView(Context context, QuestionViewModel model) : base(context, model)
        {
        }*/

        public NumericQuestionView(Context context, IMvxBindingActivity bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source, questionnairePublicKey)
        {
        }

       /* public NumericQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionViewModel model) : base(context, attrs, defStyle, model)
        {
        }

        public NumericQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionViewModel model) : base(javaReference, transfer, model)
        {
        }*/

        protected override void Initialize()
        {
            base.Initialize();
            llWrapper.Click += NumericQuestionView_Click;
            etAnswer=new EditText(this.Context);
            etAnswer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            etAnswer.Text = Model.AnswerString;
            etAnswer.InputType = InputTypes.ClassNumber;
            
            etAnswer.SetSelectAllOnFocus(true);
            etAnswer.ImeOptions=ImeAction.Done;
            etAnswer.SetSingleLine(true);
            etAnswer.EditorAction += etAnswer_EditorAction;
            llWrapper.AddView(etAnswer);
        }
        void etAnswer_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            CommandService.Execute(new SetAnswerCommand(this.QuestionnairePublicKey, Model.PublicKey.PublicKey,
                                                       null, etAnswer.Text,
                                                       Model.PublicKey.PropagationKey));
            etAnswer.ClearFocus();
            InputMethodManager imm
                = (InputMethodManager)this.Context.GetSystemService(
                    Context.InputMethodService);
            imm.HideSoftInputFromWindow(etAnswer.WindowToken, 0);
        }
        void NumericQuestionView_Click(object sender, EventArgs e)
        {
            etAnswer.RequestFocus();
        }

        protected TextView tvTitle
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvTitle); }
        }

        protected EditText etAnswer { get; set; }
    }
}