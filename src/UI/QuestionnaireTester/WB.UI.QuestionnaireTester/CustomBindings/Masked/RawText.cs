using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.UI.QuestionnaireTester.CustomBindings.Masked
{

    /**
     * Raw text, another words TextWithout mask characters
     */
    public class RawText
    {
        public readonly char EmptyChar = '\0';

        private char[] text;

        public RawText(int count)
        {
            this.text = new char[count];
        }

        /**
          * text = 012345678, range = 123 => text = 0456789
          * @param range
          */
        public void SubtractFromString(Range range)
        {
            for (int i = range.Start; i <= range.End && i < text.Length; i++)
            {
                text[i] = EmptyChar;
            }
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
            for (int i = 0; i < newString.Length; i++)
            {
                var index = start + i;
                if (index < text.Length)
                {
                    this.text[index] = newString[i];
                }
                else
                {
                    return i;
                }
            }

            return newString.Length;
        }

        public int Length
        {
            get { return this.text.Length; }
        }

        public bool HasAnyText
        {
            get { return text.Any(c => c != EmptyChar); }
        }

        public bool IsAnswered
        {
            get { return text.All(c => c != EmptyChar); }
        }

        public char CharAt(int position)
        {
            return this.text[position];
        }
    }
}