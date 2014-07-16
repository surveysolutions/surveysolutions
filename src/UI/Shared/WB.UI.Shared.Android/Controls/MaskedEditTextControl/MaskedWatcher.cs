using System;
using Android.Text;
using Android.Widget;
using Java.Lang;
using String = System.String;

namespace WB.UI.Shared.Android.Controls.MaskedEditTextControl
{
    public class MaskedWatcher : Java.Lang.Object, ITextWatcher
    {
        private readonly MaskedFormatter mMaskFormatter;
        private readonly EditText editor;
        public MaskedWatcher(String mask, EditText editor)
        {
            this.editor = editor;
            mMaskFormatter = new MaskedFormatter(mask);
        }

        public String Mask
        {
            get { return this.mMaskFormatter.Mask; }
            set { this.mMaskFormatter.Mask = value; }
        }

        public bool IsTextMaskMatched()
        {
            return mMaskFormatter.IsTextMaskMatched(editor.Text);
        }

        public void AfterTextChanged(IEditable s)
        {
            int cursorPosition = editor.SelectionEnd;
            var filtered = mMaskFormatter.ValueToString(s, ref cursorPosition);
            if (!string.Equals(s.ToString(), filtered))
            {
                s.Replace(0, s.Length(), filtered);
                if (cursorPosition <= s.Length())
                    editor.SetSelection(cursorPosition);
            }
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count) {}
    }
}