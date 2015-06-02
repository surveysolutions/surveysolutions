using System.Text.RegularExpressions;
using Android.Provider;
using Android.Text;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Java.Lang;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;
using String = System.String;


namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class EditTextMaskBinding : BaseBinding<EditText, string>
    {
        private class MaskedWatcher : Java.Lang.Object, ITextWatcher
        {
            private readonly IMaskedFormatter maskFormatter;
            private readonly EditText editor;
            private string previousValue = null;

            public MaskedWatcher(String mask, EditText editor)
            {
                this.editor = editor;
                if (string.IsNullOrEmpty(mask))
                    this.maskFormatter = new EmptyMaskFormatter();
                else
                    this.maskFormatter = new MaskedFormatter(mask);
            }

            public void AfterTextChanged(IEditable s)
            {
                string newValue = s.ToString();

                if (previousValue == newValue)
                    return;

                try
                {
                    int cursorPosition = editor.SelectionEnd;
                    var filtered = this.maskFormatter.FormatValue(newValue ?? "", ref cursorPosition);
                    if (string.Equals(newValue, filtered))
                        return;

                    previousValue = filtered;
                    s.Replace(0, s.Length(), filtered);
                    editor.SetSelection(cursorPosition);
                }
                catch (System.Exception e) { }
            }

            public void BeforeTextChanged(Java.Lang.ICharSequence s, int start, int count, int after) { }

            public void OnTextChanged(Java.Lang.ICharSequence s, int start, int before, int count) { }
        }

        public class MaskInputFilter : Java.Lang.Object, IInputFilter
        {
            private Regex mPattern;

            public MaskInputFilter(string pattern)
            {
                mPattern =  new Regex(pattern, RegexOptions.Compiled);  
            } 

            public ICharSequence FilterFormatted (ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
            //public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
            {
                string textToCheck = dest.SubSequence(0, dstart)
                    + source.SubSequence(start, end) 
                    + dest.SubSequence(dend, dest.Length());

                Match match = mPattern.Match(textToCheck);

                // Entered text does not match the pattern  
                if (!match.Success)
                {
                    return new Java.Lang.String("");
                }

                return null;
            }
         }

        public EditTextMaskBinding(EditText target)
            : base(target)
        {
        }

        protected override void SetValueToView(EditText view, string value)
        {
            bool isInputMasked = !string.IsNullOrWhiteSpace(value);

            if (isInputMasked)
            {
                view.SetFilters(new IInputFilter[] { new MaskInputFilter(value) });
                //arget.TextChanged += TargetOnTextChanged;
                //var maskedWatcher = new MaskedWatcher(value, Target);
                //Target.AddTextChangedListener(maskedWatcher);
                //Target.InputType = InputTypes.TextVariationVisiblePassword; //fix for samsung 
            }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }
    }
}
