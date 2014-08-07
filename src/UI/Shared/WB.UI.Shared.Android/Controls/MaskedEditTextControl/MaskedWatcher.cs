using System;
using Android.Text;
using Android.Widget;
using Java.Lang;
using WB.Core.BoundedContexts.Capi.UI.MaskFormatter;
using String = System.String;

namespace WB.UI.Shared.Android.Controls.MaskedEditTextControl
{
    public class MaskedWatcher : Java.Lang.Object, ITextWatcher
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

        public String Mask
        {
            get { return this.maskFormatter.Mask; }
        }

        public bool IsTextMaskMatched()
        {
            return this.maskFormatter.IsTextMaskMatched(editor.Text);
        }

        public void AfterTextChanged(IEditable s)
        {
            if (previousValue == editor.Text)
                return;
            
            int cursorPosition = editor.SelectionEnd;
            var filtered = this.maskFormatter.FormatValue(editor.Text??"", ref cursorPosition);
            if (!string.Equals(editor.Text, filtered))
            {
                previousValue = filtered;
                s.Replace(0, s.Length(), filtered);
                editor.SetSelection(cursorPosition);
            }
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count) {}
    }
}