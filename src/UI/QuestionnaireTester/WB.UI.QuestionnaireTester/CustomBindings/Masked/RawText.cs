using System;

namespace WB.UI.QuestionnaireTester.CustomBindings.Masked
{

    /**
     * Raw text, another words TextWithout mask characters
     */
    public class RawText
    {
        private string text;

        public RawText()
        {
            this.text = "";
        }

        /**
          * text = 012345678, range = 123 => text = 0456789
          * @param range
          */
        public void SubtractFromString(Range range)
        {
            String firstPart = "";
            String lastPart = "";

            if (range.Start > 0 && range.Start <= this.text.Length)
            {
                firstPart = this.text.Substring(0, range.Start);
            }
            if (range.End >= 0 && range.End < this.text.Length)
            {
                lastPart = this.text.Substring(range.End, this.text.Length - range.End);
            }
            this.text = firstPart + lastPart;
        }

        /**
         * 
         * @param newString New String to be added
         * @param start Position to insert newString
         * @param maxLength Maximum raw text length
         * @return Number of added characters
         */
        public int AddToString(string newString, int start, int maxLength)
        {
            String firstPart = "";
            String lastPart = "";

            if (newString == null || newString.Equals(""))
            {
                return 0;
            }
            else if (start < 0)
            {
                throw new ArgumentException("Start position must be non-negative");
            }
            else if (start > this.text.Length)
            {
                throw new ArgumentException("Start position must be less than the actual text length");
            }

            int count = newString.Length;

            if (start > 0)
            {
                firstPart = this.text.Substring(0, start);
            }
            if (start >= 0 && start < this.text.Length)
            {
                lastPart = this.text.Substring(start, this.text.Length - start);
            }
            if (this.text.Length + newString.Length > maxLength)
            {
                count = maxLength - this.text.Length;
                newString = newString.Substring(0, count);
            }
            this.text = firstPart + newString + lastPart;
            return count;
        }

        public int Length
        {
            get { return this.text.Length; }
        }

        public char CharAt(int position)
        {
            return this.text[position];
        }
    }
}