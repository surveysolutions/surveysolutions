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
        private readonly IMaskedFormatter mMaskFormatter;
        private readonly EditText editor;
        public MaskedWatcher(String mask, EditText editor)
        {
            this.editor = editor;
            if (string.IsNullOrEmpty(mask))
                mMaskFormatter = new EmptyMaskFormatter();
            else
                mMaskFormatter = new MaskedFormatter(mask);
        }

        public String Mask
        {
            get { return this.mMaskFormatter.Mask; }
        }

        public bool IsTextMatchesToMask()
        {
            return mMaskFormatter.IsTextMaskMatched(editor.Text);
        }

        public string GetCleanText()
        {
            return mMaskFormatter.GetCleanText(editor.Text);
        }

        public void AfterTextChanged(IEditable s)
        {
            int cursorPosition = editor.SelectionEnd;
            var filtered = mMaskFormatter.ValueToString(editor.Text, ref cursorPosition);
            if (!string.Equals(editor.Text, filtered))
            {
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