using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Text;
using Android.Util;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;

namespace WB.UI.Shared.Enumerator.CustomControls.MaskedEditTextControl
{
    public class MaskedEditText : EditText, IInputFilter
    {
        private MaskedText maskedText = new MaskedText();

        private bool editingBefore;
        private bool editingOnChanged;
        private bool editingAfter;
        private int selection;
        private int lastValidMaskPosition;
        private bool initialized;
        private bool ignore;
        private bool selectionChanged;
    
        public MaskedEditText(Context context) : base(context) 
        {
            this.Init();
        }

        public MaskedEditText(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            this.Init();

            this.CleanUp();
        }

        private void CleanUp()
        {
            if (this.Mask.IsNullOrEmpty()) 
                return;

            this.initialized = false;

            this.maskedText.Init();

            this.selection = this.maskedText.FindFirstValidMaskPosition();
            this.lastValidMaskPosition = this.maskedText.FindLastValidMaskPosition();
            this.LongClickable = string.IsNullOrEmpty(this.maskedText?.Mask);

            this.initialized = true;
        }


        public MaskedEditText(Context context, IAttributeSet attrs, int defStyle)
            :base(context, attrs, defStyle)
        {
            this.Init();
        }

        public string Mask
        {
            get { return this.maskedText.Mask; }
            set
            {
                this.maskedText = new MaskedText();
                this.maskedText.Mask = value;
                this.CleanUp();

                this.UpdateInputType();
            }
        }

        private void UpdateInputType()
        {
            var inputTypes = this.InputType;
            var inputTypeOverride = InputTypes.TextFlagNoSuggestions | InputTypes.TextVariationPassword;

            if (this.Mask.IsNullOrEmpty())
            {
                inputTypes &= ~inputTypeOverride;
                this.SetMaxLines(int.MaxValue);
                this.SetSingleLine(false);
            }
            else
            {
                inputTypes |= inputTypeOverride;
            }

            this.SetRawInputType(inputTypes);
        }

        public event EventHandler<EventArgs> IsMaskedFormAnsweredChanged; 

        private bool isMaskedFormAnswered;
        public bool IsMaskedFormAnswered
        {
            get { return this.isMaskedFormAnswered; }
            private set
            {
                if (this.isMaskedFormAnswered != value)
                {
                    this.isMaskedFormAnswered = value;
                    this.IsMaskedFormAnsweredChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private bool HasHint
        {
            get { return !string.IsNullOrEmpty(this.Hint); }
        }

        private void Init()
        {
            this.SetFilters(new IInputFilter[] { this, new  InputFilterLengthFilter (AnswerUtils.TextAnswerMaxLength) });
            this.AfterTextChanged += (sender, args) => this.AfterTextChangedHandler(args.Editable);
            this.BeforeTextChanged += (sender, args) => this.BeforeTextChangedHandler(args.Text, args.Start, args.BeforeCount, args.AfterCount);
            this.TextChanged += (sender, args) => this.OnTextChangedHandle(new string(args.Text.ToArray()), args.Start, args.BeforeCount, args.AfterCount);

            this.UpdateInputType();

            this.FocusChange += this.OnFocusChangeHandle;
        }

        private void OnFocusChangeHandle(object sender, FocusChangeEventArgs args)
        {
            if (this.Mask.IsNullOrEmpty())
                return;

            if (!this.maskedText.HasAnyText && this.HasHint)
            {
                if (args.HasFocus)
                {
                    this.EditableText.Replace(0, this.Text.Length, this.maskedText.MakeMaskedText());
                    var position = this.maskedText.FindFirstValidMaskPosition();
                    this.SetSelection(position);

                    InputMethodManager imm = (InputMethodManager)this.Context.GetSystemService(Context.InputMethodService);
                    imm.ShowSoftInput(this, ShowFlags.Implicit);
                }
                else
                {
                    this.selection = 0;
                    this.EditableText.Clear();
                }
            }
        }

        protected override void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            this.OnTextChangedHandle(s.ToString(), start, before, count);
        }

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            if (this.Mask.IsNullOrEmpty())
                return null;

            if (this.editingAfter)
                return null;

            if (source.Length() == 0)
                return null;

            var filteredString = this.maskedText.Filter(source.ToString(), dstart);
            if (filteredString == null)
                return null;

            return new Java.Lang.String(filteredString);
        }

        void BeforeTextChangedHandler(IEnumerable<char> s, int start, int count, int after)
        {
            if (this.Mask.IsNullOrEmpty())
                return;

            if (!this.editingBefore)
            {
                this.editingBefore = true;
                if (start > this.lastValidMaskPosition)
                {
                    this.ignore = true;
                }

                this.maskedText.RemoveRange(start, count, after, ref this.selection);
            }
        }

        void OnTextChangedHandle(string s, int start, int before, int count)
        {
            if (this.Mask.IsNullOrEmpty())
                return;

            if (!this.editingOnChanged && this.editingBefore)
            {
                this.editingOnChanged = true;

                if (this.ignore)
                    return;

                if (count > 0)
                {
                    string addedString = s.Substring(start, count);
                    this.maskedText.AddString(addedString, start, ref this.selection);
                }
            }

            this.SetSelectAllOnFocus(!this.maskedText.HasAnyText);
        }

        void AfterTextChangedHandler(IEditable s)
        {
            if (this.Mask.IsNullOrEmpty())
                return;

            if(!this.editingAfter && this.editingBefore && this.editingOnChanged) 
            {
                this.editingAfter = true;
                if (!this.maskedText.HasAnyText && this.HasHint && !this.HasFocus) 
                {
                    this.selection = 0;
                    this.EditableText.Clear();
                }
                else
                {
                    this.EditableText.Replace(0, this.Text.Length, this.maskedText.MakeMaskedText());
                }

                this.IsMaskedFormAnswered = this.maskedText.IsMaskedFormAnswered;
            
                this.selectionChanged = false;
                this.SetSelection(this.selection);

                this.editingBefore = false;
                this.editingOnChanged = false;
                this.editingAfter = false;
                this.ignore = false;
            }
        }
    
        protected override void OnSelectionChanged(int selStart, int selEnd) 
        {
            // On Android 4+ this method is being called more than 1 time if there is a hint in the EditText, what moves the cursor to left
            // Using the bool var selectionChanged to limit to one execution
            if (this.Mask.IsNullOrEmpty()) 
            {
                base.OnSelectionChanged(selStart, selEnd);
                return;
            }

            if(this.initialized )
            {
                if(!this.selectionChanged) 
                {
                    if (!this.maskedText.HasAnyText && this.HasHint)
                    {
                        selStart = 0;
                        selEnd = 0;
                    }
                    else
                    {
                        selStart = this.maskedText.FixSelectionIndex(selStart);
                        selEnd = this.maskedText.FixSelectionIndex(selEnd);
                    }

                    if (this.Text.Length > selStart && this.Text.Length > selEnd)
                    {
                        this.SetSelection(selStart, selEnd);
                        this.selectionChanged = true;
                    }
                }
                else  //check to see if the current selection is outside the already entered text
                {
                    if(!(this.HasHint && this.maskedText.RawTextLength == 0)
                        && selStart > this.maskedText.RawTextLength - 1)
                    {
                        this.SetSelection(
                            this.maskedText.FixSelectionIndex(selStart), 
                            this.maskedText.FixSelectionIndex(selEnd));
                    }
                }
            }

            base.OnSelectionChanged(selStart, selEnd);
        }
    }
}
