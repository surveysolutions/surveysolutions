using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using WB.Core.GenericSubdomains.Portable;
using String = System.String;


namespace WB.UI.QuestionnaireTester.CustomControls.MaskedEditTextControl
{
    public class MaskedEditText : EditText, ITextWatcher, IInputFilter
    {
        private const char DigitKey = '#';
        private const char CharacterKey = '~';
        private const char AnythingKey = '*';

        private readonly char[] charRepresentationArray = { AnythingKey, DigitKey, CharacterKey };

        private String mask = String.Empty;
        private char maskFill = '_';

        private int[] rawToMask;
        private RawText rawText;
        private bool editingBefore;
        private bool editingOnChanged;
        private bool editingAfter;
        private int?[] maskToRaw;
        private char[] charsInMask;
        private int selection;
        private bool initialized;
        private bool ignore;
        private int maxRawLength;
        private int lastValidMaskPosition;
        private bool selectionChanged;
        private IOnFocusChangeListener focusChangeListener;
    
        public MaskedEditText(Context context) : base(context) 
        {
            this.Init();
        }

        public MaskedEditText(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            this.Init();

            this.CleanUp();

            // Ignoring enter key presses
            this.EditorAction += (sender, args) =>
            {
                args.Handled = args.ActionId != ImeAction.Next;
            };
        }

        /** @param listener - its onFocusChange() method will be called before performing MaskedEditText operations, 
         * related to this event. */
        public void SetOnFocusChangeListener(IOnFocusChangeListener listener) 
        {
            this.focusChangeListener = listener;
        }
    
        private void CleanUp() 
        {
            if (this.mask.IsNullOrEmpty()) 
                return;

            this.initialized = false;
        
            this.GeneratePositionArrays();

            this.maxRawLength = this.maskToRaw[this.PreviousValidPosition(this.mask.Length - 1)].GetValueOrDefault(-1) + 1;
            this.rawText = new RawText(this.maxRawLength);
            this.selection = this.rawToMask[0];

            this.editingBefore = true;
            this.editingOnChanged = true;
            this.editingAfter = true;

            if(this.HasHint) 
            {
                this.EditableText.Clear();
            }
            else 
            {
                this.EditableText.Clear();
                this.EditableText.Append(this.ReplaceCharRepresentation(this.mask, this.maskFill));
            }

            this.editingBefore = false;
            this.editingOnChanged = false;
            this.editingAfter = false;
        
            this.lastValidMaskPosition = this.FindLastValidMaskPosition();
            this.initialized = true;

            this.FocusChange += (sender, args) =>
            {
                if (this.focusChangeListener != null) 
                {
                    this.focusChangeListener.OnFocusChange((View)sender, this.HasFocus);
                }

                if (args.HasFocus && (this.rawText.HasAnyText || !this.HasHint)) 
                {
                    this.selectionChanged = false;
                    var lastValidPosition = this.LastValidPosition();

                    if (lastValidPosition < Text.Length)
                    {
                        this.SetSelection(lastValidPosition);
                    }
                }
            };
        }

        private int FindLastValidMaskPosition() 
        {
            for(int i = this.maskToRaw.Length - 1; i >= 0; i--) 
            {
                if(this.maskToRaw[i].HasValue) 
                    return i;
            }

            throw new RuntimeException("Mask contains only the representation char");
        }

        public MaskedEditText(Context context, IAttributeSet attrs, int defStyle)
            :base(context, attrs, defStyle)
        {
            this.Init();
        }

        public string Mask
        {
            get { return this.mask; }
            set
            {
                if (this.mask != value)
                {
                    this.mask = value;
                    this.CleanUp();
                }
            }
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

                    if (this.IsMaskedFormAnsweredChanged != null)
                    {
                        this.IsMaskedFormAnsweredChanged.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        private bool HasHint
        {
            get { return this.Hint != null; }
        }

        private void GeneratePositionArrays() 
        {
            int[] aux = new int[this.mask.Length];
            this.maskToRaw = new int?[this.mask.Length];
            String charsInMaskAux = "";
        
            int charIndex = 0;
            for(int i = 0; i < this.mask.Length; i++) 
            {
                char currentChar = this.mask[i];
                if(this.charRepresentationArray.Contains(currentChar)) 
                {
                    aux[charIndex] = i;
                    this.maskToRaw[i] = charIndex++;
                }
                else 
                {
                    String charAsString = Character.ToString(currentChar);
                    if(!charsInMaskAux.Contains(charAsString) 
                        && !Character.IsLetter(currentChar) 
                        && !Character.IsDigit(currentChar)) 
                    {
                        charsInMaskAux = charsInMaskAux + charAsString;
                    }
                    this.maskToRaw[i] = null;
                }
            }

            if(charsInMaskAux.IndexOf(' ') < 0) 
            {
                charsInMaskAux = charsInMaskAux + " ";
            }

            this.charsInMask = charsInMaskAux.ToCharArray();
        
            this.rawToMask = new int[charIndex];
            for (int i = 0; i < charIndex; i++) 
            {
                this.rawToMask[i] = aux[i];
            }
        }
    
        private void Init() 
        {
            this.SetFilters(new IInputFilter[] { this });
            //this.AddTextChangedListener(this);
            this.AfterTextChanged += (sender, args) => AfterTextChangedHandler(args.Editable);
            this.BeforeTextChanged += (sender, args) => BeforeTextChangedHandler(args.Text, args.Start, args.BeforeCount, args.AfterCount);
            this.TextChanged += (sender, args) => OnTextChangedHandle(new string(args.Text.ToArray()), args.Start, args.BeforeCount, args.AfterCount);

            this.SetRawInputType(InputTypes.TextFlagNoSuggestions);
        }

        void ITextWatcher.AfterTextChanged(IEditable s)
        {
            this.AfterTextChangedHandler(s);
        }

        void ITextWatcher.BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            this.BeforeTextChangedHandler(s, start, count, after);
        }

        void ITextWatcher.OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            this.OnTextChanged(s, start, before, count);
        }

        protected override void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            this.OnTextChangedHandle(s.ToString(), start, before, count);
        }

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            if (this.mask.IsNullOrEmpty())
                return null;

            if (this.editingAfter)
                return null;

            if (source.Length() == 0)
                return null;

            char[] filterArray = new char[source.Length()];

            var indexInMask = dstart;

            for (int i = 0; i < source.Length(); i++)
            {
                var currentChar = source.CharAt(i);

                indexInMask = this.NextValidPosition(indexInMask);
                if (indexInMask >= this.mask.Length)
                    continue;

                var maskChar = this.mask[indexInMask];

                switch (maskChar)
                {
                    case DigitKey:
                        filterArray[i] = char.IsDigit(currentChar) ? currentChar : this.maskFill;
                        break;

                    case CharacterKey:
                        filterArray[i] = char.IsLetter(currentChar) ? currentChar : this.maskFill;
                        break;

                    case AnythingKey:
                        filterArray[i] = currentChar;
                        break;

                    default:
                        filterArray[i] = maskChar;
                        break;
                }
            }

            var filterString = new string(filterArray);
            filterString = filterString.Replace(this.maskFill.ToString(), "");
            return new Java.Lang.String(filterString);
        }

        void BeforeTextChangedHandler(IEnumerable<char> s, int start, int count, int after)
        {
            if (this.mask.IsNullOrEmpty())
                return;

            if (!this.editingBefore)
            {
                this.editingBefore = true;
                if (start > this.lastValidMaskPosition)
                {
                    this.ignore = true;
                }

                int rangeStart = start;
                int rangeEnd = start + count;

                if (after == 0)
                {
                    rangeStart = this.ErasingStart(start);
                    rangeEnd = start + after;
                }

                Range range = this.CalculateRange(rangeStart, rangeEnd);
                if (range.Start.HasValue)
                {
                    this.rawText.SubtractFromString(range);
                }

                if (count > 0)
                {
                    this.selection = this.PreviousValidPosition(start);
                }
            }
        }

        protected void OnTextChangedHandle(string s, int start, int before, int count)
        {
            if (this.mask.IsNullOrEmpty())
                return;

            if (!this.editingOnChanged && this.editingBefore)
            {
                this.editingOnChanged = true;

                if (this.ignore)
                    return;

                if (count > 0)
                {
                    int startingPosition = this.maskToRaw[this.NextValidPosition(start)].GetValueOrDefault(-1);
                    String addedString = s.Substring(start, count);
                    var newString = this.Clear(addedString);
                    count = this.rawText.AddToString(newString, startingPosition, this.maxRawLength);

                    if (this.initialized)
                    {
                        int currentPosition = startingPosition + count < this.rawToMask.Length
                            ? this.rawToMask[startingPosition + count]
                            : this.lastValidMaskPosition + 1;
                        this.selection = this.NextValidPosition(currentPosition);
                    }
                }
            }
        }

        void AfterTextChangedHandler(IEditable s)
        {
            if (this.mask.IsNullOrEmpty()) 
                return;

            if(!this.editingAfter && this.editingBefore && this.editingOnChanged) 
            {
                this.editingAfter = true;
                if (!this.rawText.HasAnyText && this.HasHint) 
                {
                    this.selection = 0;
                    this.EditableText.Clear(); 
                }
                else 
                {
                    this.EditableText.Clear();
                    this.EditableText.Append(this.MakeMaskedText());
                }

                this.IsMaskedFormAnswered = this.rawText.IsAnswered;
            
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
            if (this.mask.IsNullOrEmpty()) 
            {
                base.OnSelectionChanged(selStart, selEnd);
                return;
            }

            if(this.initialized )
            {
                if(!this.selectionChanged) 
                {
                    if (!this.rawText.HasAnyText && this.HasHint) 
                    {
                        selStart = 0;
                        selEnd = 0;
                    }
                    else 
                    {
                        selStart = this.FixSelection(selStart);
                        selEnd = this.FixSelection(selEnd);
                    }

                    if (this.Text.Length > selStart && this.Text.Length > selEnd)
                    {
                        this.SetSelection(selStart, selEnd);
                        this.selectionChanged = true;
                    }
                }
                else  //check to see if the current selection is outside the already entered text
                {
                    if(!(this.HasHint && this.rawText.Length == 0) 
                        && selStart > this.rawText.Length - 1)
                    {
                        this.SetSelection(this.FixSelection(selStart),this.FixSelection(selEnd));
                    }
                }
            }

            base.OnSelectionChanged(selStart, selEnd);
        }


        private int ErasingStart(int start)
        {
            while (start > 0 && !this.maskToRaw[start].HasValue)
            {
                start--;
            }

            return start;
        }
    
        private int FixSelection(int selectionPosition) 
        {
            if(selectionPosition > this.LastValidPosition()) 
                return this.LastValidPosition();

            return this.NextValidPosition(selectionPosition);
        }

        private int NextValidPosition(int currentPosition) 
        {
            while(currentPosition < this.lastValidMaskPosition && !this.maskToRaw[currentPosition].HasValue) 
            {
                currentPosition++;
            }

            if(currentPosition > this.lastValidMaskPosition) 
                return this.lastValidMaskPosition + 1;

            return currentPosition;
        }
    
        private int PreviousValidPosition(int currentPosition) 
        {
            while(currentPosition >= 0 && !this.maskToRaw[currentPosition].HasValue) 
            {
                currentPosition--;

                if(currentPosition < 0) 
                    return this.NextValidPosition(0);
            }

            return currentPosition;
        }
    
        private int LastValidPosition() 
        {
            if(this.rawText.Length == this.maxRawLength) 
                return this.rawToMask[this.rawText.Length - 1] + 1;

            return this.NextValidPosition(this.rawToMask[this.rawText.Length]);
        }
    
        private String MakeMaskedText() 
        {
            char[] maskedText = this.ReplaceCharRepresentation(this.mask, ' ').ToCharArray();

            for(int i = 0; i < this.rawToMask.Length; i++)
            {
                var rawTextChar = this.rawText.CharAt(i);

                if (rawTextChar != this.rawText.EmptyChar)
                {
                    maskedText[this.rawToMask[i]] = rawTextChar;
                }
                else
                {
                    maskedText[this.rawToMask[i]] = this.maskFill;
                }
            }
            return new String(maskedText);
        }

        private Range CalculateRange(int start, int end) 
        {
            Range range = new Range();
            for(int i = start; i <= end && i < this.mask.Length; i++) 
            {
                if(this.maskToRaw[i].HasValue) 
                {
                    if(!range.Start.HasValue) 
                    {
                        range.Start = this.maskToRaw[i];
                    }
                    range.End = this.maskToRaw[i];
                }
            }

            if(end == this.mask.Length) 
            {
                range.End = this.rawText.Length;
            }

            return range;
        }
    
        private String Clear(String str) 
        {
            str = str.Replace(Character.ToString(this.maskFill), String.Empty);

            return str;
        }

        private string ReplaceCharRepresentation(string originalString, char replaceChar)
        {
            return this.charRepresentationArray.Aggregate(
                originalString, 
                (current, charRepresentation) => current.Replace(charRepresentation, replaceChar)
                );
        }
    }
}