using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Tester.Services.MaskText
{
    public class MaskedText
    {
        private char maskFill = '_';

        private const char DigitKey = '#';
        private const char CharacterKey = '~';
        private const char AnythingKey = '*';

        private readonly char[] charRepresentationArray = { AnythingKey, DigitKey, CharacterKey };

        private int[] rawToMask;
        private RawText rawText;
        private int?[] maskToRaw;
        private char[] charsInMask;
        private bool initialized;
        private int maxRawLength;
        private int lastValidMaskPosition;

        private string mask;

        public string Mask
        {
            get { return this.mask; }
            set
            {
                this.mask = value;
                this.Init();
            }
        }

        public bool IsMaskedFormAnswered
        {
            get { return this.rawText.IsAnswered; }
        }

        public bool HasAnyText
        {
            get { return this.rawText.HasAnyText; }
        }

        public int RawTextLength
        {
            get { return this.rawText.Length; }
        }

        public void Init()
        {
            if (this.mask.IsNullOrEmpty())
                return;

            this.initialized = false;

            this.GeneratePositionArrays();

            this.maxRawLength = this.maskToRaw[this.PreviousValidPosition(this.mask.Length - 1)].GetValueOrDefault(-1) + 1;
            this.rawText = new RawText(this.maxRawLength);
            this.lastValidMaskPosition = this.FindLastValidMaskPosition();

            this.initialized = true;
        }

        public void RemoveRange(int start, int count, int after, ref int selection)
        {
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
                selection = this.PreviousValidPosition(start);
            }
        }


        public void AddString(string addedString, int insertPosition, ref int position)
        {
            if (addedString.IsNullOrEmpty())
                return;

            addedString = FilterOnlyMaskedChars(addedString, insertPosition);

            int startingPosition = this.maskToRaw[this.NextValidPosition(insertPosition)].GetValueOrDefault(-1);
            var newString = this.Clear(addedString);
            int count = this.rawText.AddToString(newString, startingPosition, this.maxRawLength);

            if (this.initialized)
            {
                int currentPosition = startingPosition + count < this.rawToMask.Length
                    ? this.rawToMask[startingPosition + count]
                    : this.lastValidMaskPosition + 1;
                position = this.NextValidPosition(currentPosition);
            }
        }


        public string Filter(string source, int position)
        {
            return this.FilterImpl(source, position, false);
        }

        public string FilterOnlyMaskedChars(string source, int position)
        {
            return this.FilterImpl(source, position, true);
        }


        private string FilterImpl(string source, int position, bool skipNonMaskedChars)
        {
            if (this.Mask.IsNullOrEmpty())
                return null;

            if (source.Length == 0)
                return null;

            char[] filterArray = new char[source.Length];

            var indexInMask = position;

            for (int i = 0; i < source.Length; i++)
            {
                var currentChar = source[i];

                var nextValidPosition = this.NextValidPosition(indexInMask);
                if (nextValidPosition >= this.Mask.Length)
                    continue;

                if (nextValidPosition > indexInMask)
                {
                    var maskCharForCheck = this.Mask[indexInMask];

                    if (maskCharForCheck == currentChar)
                    {
                        indexInMask++;

                        if (!skipNonMaskedChars)
                            filterArray[i] = currentChar;
                            
                        continue;
                    }
                }

                indexInMask = nextValidPosition;
                var maskChar = this.Mask[indexInMask];

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

                indexInMask++;
            }

            var filterString = new string(filterArray);
            filterString = filterString
                //.Replace(this.maskFill.ToString(), "")
                .Replace('\0'.ToString(), "");
            return filterString;
        }


        public int FindLastValidMaskPosition()
        {
            for (int i = this.maskToRaw.Length - 1; i >= 0; i--)
            {
                if (this.maskToRaw[i].HasValue)
                    return i;
            }

            throw new ArgumentException("Mask contains only the representation char");
        }

        public int FindFirstValidMaskPosition()
        {
            if (this.rawToMask.Any())
                return this.rawToMask[0];

            return 0;
        }

        public int FixSelectionIndex(int selectionPosition)
        {
            if (selectionPosition > this.LastValidPosition())
                return this.LastValidPosition();

            return this.NextValidPosition(selectionPosition);
        }



        private void GeneratePositionArrays()
        {
            int[] aux = new int[this.mask.Length];
            this.maskToRaw = new int?[this.mask.Length];
            String charsInMaskAux = "";

            int charIndex = 0;
            for (int i = 0; i < this.mask.Length; i++)
            {
                char currentChar = this.mask[i];
                if (this.charRepresentationArray.Contains(currentChar))
                {
                    aux[charIndex] = i;
                    this.maskToRaw[i] = charIndex++;
                }
                else
                {
                    String charAsString = char.ToString(currentChar);
                    if (!charsInMaskAux.Contains(charAsString)
                        && !char.IsLetter(currentChar)
                        && !char.IsDigit(currentChar))
                    {
                        charsInMaskAux = charsInMaskAux + charAsString;
                    }
                    this.maskToRaw[i] = null;
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


        private int ErasingStart(int start)
        {
            while (start > 0 && !this.maskToRaw[start].HasValue)
            {
                start--;
            }

            return start;
        }


        private int NextValidPosition(int currentPosition)
        {
            while (currentPosition < this.lastValidMaskPosition && !this.maskToRaw[currentPosition].HasValue)
            {
                currentPosition++;
            }

            if (currentPosition > this.lastValidMaskPosition)
                return this.lastValidMaskPosition + 1;

            return currentPosition;
        }

        private int PreviousValidPosition(int currentPosition)
        {
            while (currentPosition >= 0 && !this.maskToRaw[currentPosition].HasValue)
            {
                currentPosition--;

                if (currentPosition < 0)
                    return this.NextValidPosition(0);
            }

            return currentPosition;
        }

        private int LastValidPosition()
        {
            if (this.rawText.Length == this.maxRawLength)
                return this.rawToMask[this.rawText.Length - 1] + 1;

            return this.NextValidPosition(this.rawToMask[this.rawText.Length]);
        }

        public string MakeMaskedText()
        {
            char[] maskedText = this.ReplaceCharRepresentation(this.mask, ' ').ToCharArray();

            for (int i = 0; i < this.rawToMask.Length; i++)
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
            return new string(maskedText);
        }

        private Range CalculateRange(int start, int end)
        {
            Range range = new Range();
            for (int i = start; i <= end && i < this.mask.Length; i++)
            {
                if (this.maskToRaw[i].HasValue)
                {
                    if (!range.Start.HasValue)
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

        private string Clear(String str)
        {
            str = str.TrimEnd(this.maskFill).Replace(this.maskFill, rawText.EmptyChar);
            return str;
        }

        private string ReplaceCharRepresentation(string originalString, char replaceChar)
        {
            if (originalString.IsNullOrEmpty())
                return originalString;

            return this.charRepresentationArray.Aggregate(
                originalString,
                (current, charRepresentation) => current.Replace(charRepresentation, replaceChar)
                );
        }
    }
}