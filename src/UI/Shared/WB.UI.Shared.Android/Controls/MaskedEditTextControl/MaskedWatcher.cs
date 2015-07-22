using Android.Text;
using Android.Widget;
using Java.Lang;
using Microsoft.Practices.ServiceLocation;

using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;
using String = System.String;

namespace WB.UI.Shared.Android.Controls.MaskedEditTextControl
{
    public class MaskedWatcher : Java.Lang.Object, ITextWatcher
    {
        private readonly IMaskedFormatter maskFormatter;
        private readonly EditText editor;
        private string previousValue = null;

        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

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
            catch (System.Exception e)
            {
                Logger.Error(e.Message, e);
            }
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count) {}
    }
}