using System;
using System.Collections.Generic;
using System.Linq;
using Android.Text;
using Android.Widget;
using Java.Lang;
using String = System.String;
using WB.Core.GenericSubdomains.Portable;


namespace WB.UI.QuestionnaireTester.CustomBindings.Masked
{
    public class EditTextMaskWrapper : Java.Lang.Object, ITextWatcher, IInputFilter
    {
        private const char DigitKey = '#';
        private const char CharacterKey = '~';
        private const char AnythingKey = '*';

        private readonly char[] charRepresentationArray = { AnythingKey, DigitKey, CharacterKey };

        private String mask = String.Empty;
        private char maskFill = '_';

        private readonly EditText editText;
        private RawText rawText;
        private int[] rawToMask;
        private int[] maskToRaw;
        private char[] charsInMask;
        private int maxRawLength;

        private int selection;
        private int lastValidMaskPosition;
        private bool editingBefore;
        private bool editingOnChanged;
        private bool editingAfter;
        private bool selectionChanged;
        private bool initialized;
        private bool ignore;


        public EditTextMaskWrapper(EditText editText)
        {
            this.editText = editText;

            this.Init();

            this.CleanUp();
        }

        private void CleanUp()
        {
            if (this.mask.IsNullOrEmpty())
                return;

            this.initialized = false;

            this.GeneratePositionArrays();

            this.maxRawLength = this.maskToRaw[this.PreviousValidPosition(this.mask.Length - 1)] + 1;
            this.rawText = new RawText(maxRawLength);
            this.selection = this.rawToMask[0];

            this.editingBefore = true;
            this.editingOnChanged = true;
            this.editingAfter = true;

            if (this.HasHint)
            {
                this.editText.EditableText.Clear();
            }
            else
            {
                this.editText.EditableText.Clear();
                this.editText.EditableText.Append(ReplaceCharRepresentation(this.mask, this.maskFill));
            }

            this.editingBefore = false;
            this.editingOnChanged = false;
            this.editingAfter = false;

            this.lastValidMaskPosition = this.FindLastValidMaskPosition();
            this.initialized = true;
        }

        private int FindLastValidMaskPosition()
        {
            for (int i = this.maskToRaw.Length - 1; i >= 0; i--)
            {
                if (this.maskToRaw[i] != -1)
                    return i;
            }

            throw new RuntimeException("Mask contains only the representation char");
        }

        private bool HasHint
        {
            get { return this.editText.Hint != null; }
        }

        public string Mask
        {
            get { return this.mask; }
            set
            {
                this.mask = value;
                this.CleanUp();
            }
        }

        public bool IsAnswered
        {
            get { return rawText.IsAnswered; }
        }

        private void GeneratePositionArrays()
        {
            int[] aux = new int[this.mask.Length];
            this.maskToRaw = new int[this.mask.Length];
            String charsInMaskAux = "";

            int charIndex = 0;
            for (int i = 0; i < this.mask.Length; i++)
            {
                char currentChar = this.mask[i];
                if (charRepresentationArray.Contains(currentChar))
                {
                    aux[charIndex] = i;
                    this.maskToRaw[i] = charIndex++;
                }
                else
                {
                    String charAsString = Character.ToString(currentChar);
                    if (!charsInMaskAux.Contains(charAsString)
                        && !Character.IsLetter(currentChar)
                        && !Character.IsDigit(currentChar))
                    {
                        charsInMaskAux = charsInMaskAux + charAsString;
                    }
                    this.maskToRaw[i] = -1;
                }
            }

            if (charsInMaskAux.IndexOf(' ') < 0)
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
            this.editText.SetFilters(new IInputFilter[] { this });
            this.editText.AddTextChangedListener(this);

            this.editText.SetRawInputType(InputTypes.TextFlagNoSuggestions);
        }

        void ITextWatcher.AfterTextChanged(IEditable s)
        {
            AfterTextChangedHandler(s);
        }

        void ITextWatcher.BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            BeforeTextChangedHandler(s, start, count, after);
        }

        void ITextWatcher.OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            OnTextChangedHandle(s.ToString(), start, before, count);
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

                indexInMask = NextValidPosition(indexInMask);
                if (indexInMask >= mask.Length)
                    continue;

                var maskChar = mask[indexInMask];

                switch (maskChar)
                {
                    case DigitKey:
                        filterArray[i] = char.IsDigit(currentChar) ? currentChar : maskFill;
                        break;

                    case CharacterKey:
                        filterArray[i] = char.IsLetter(currentChar) ? currentChar : maskFill;
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
            filterString = filterString.Replace(maskFill.ToString(), "");
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
                if (range.Start != -1)
                {
                    this.rawText.SubtractFromString(range);
                }

                if (count > 0)
                {
                    this.selection = this.PreviousValidPosition(start);
                }
            }
        }

        void OnTextChangedHandle(string s, int start, int before, int count)
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
                    int startingPosition = this.maskToRaw[this.NextValidPosition(start)];
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

            if (!this.editingAfter && this.editingBefore && this.editingOnChanged)
            {
                this.editingAfter = true;
                if (!this.rawText.HasAnyText && this.HasHint)
                {
                    this.selection = 0;
                    this.editText.EditableText.Clear();
                }
                else
                {
                    this.editText.EditableText.Clear();
                    this.editText.EditableText.Append(this.MakeMaskedText());
                }

                this.selectionChanged = false;
                this.editText.SetSelection(this.selection);

                this.editingBefore = false;
                this.editingOnChanged = false;
                this.editingAfter = false;
                this.ignore = false;
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var editor = this.editText;
                if (editor != null)
                {
                    editor.RemoveTextChangedListener(this);
                }
            }
            base.Dispose(disposing);
        }

        private int ErasingStart(int start)
        {
            while (start > 0 && this.maskToRaw[start] == -1)
            {
                start--;
            }

            return start;
        }

        private int NextValidPosition(int currentPosition)
        {
            while (currentPosition < this.lastValidMaskPosition && this.maskToRaw[currentPosition] == -1)
            {
                currentPosition++;
            }

            if (currentPosition > this.lastValidMaskPosition)
                return this.lastValidMaskPosition + 1;

            return currentPosition;
        }

        private int PreviousValidPosition(int currentPosition)
        {
            while (currentPosition >= 0 && this.maskToRaw[currentPosition] == -1)
            {
                currentPosition--;

                if (currentPosition < 0)
                    return this.NextValidPosition(0);
            }

            return currentPosition;
        }

        private String MakeMaskedText()
        {
            char[] maskedText = ReplaceCharRepresentation(this.mask, ' ').ToCharArray();

            for (int i = 0; i < this.rawToMask.Length; i++)
            {
                var rawTextChar = this.rawText.CharAt(i);

                if (rawTextChar != rawText.EmptyChar)
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
            for (int i = start; i <= end && i < this.mask.Length; i++)
            {
                if (this.maskToRaw[i] != -1)
                {
                    if (range.Start == -1)
                    {
                        range.Start = this.maskToRaw[i];
                    }
                    range.End = this.maskToRaw[i];
                }
            }

            if (end == this.mask.Length)
            {
                range.End = this.rawText.Length;
            }

            return range;
        }

        private String Clear(String str)
        {
            str = str.Replace(Character.ToString(maskFill), String.Empty);

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