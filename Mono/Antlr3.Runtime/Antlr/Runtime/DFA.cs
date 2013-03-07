namespace Antlr.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class DFA
    {
        protected short[] accept;
        public const bool debug = false;
        protected int decisionNumber;
        protected short[] eof;
        protected short[] eot;
        protected char[] max;
        protected char[] min;
        protected BaseRecognizer recognizer;
        protected short[] special;
        public SpecialStateTransitionHandler specialStateTransitionHandler;
        protected short[][] transition;

        protected DFA()
        {
        }

        public virtual void Error(NoViableAltException nvae)
        {
        }

        protected void NoViableAlt(int s, IIntStream input)
        {
            if (this.recognizer.state.backtracking > 0)
            {
                this.recognizer.state.failed = true;
            }
            else
            {
                NoViableAltException nvae = new NoViableAltException(this.Description, this.decisionNumber, s, input);
                this.Error(nvae);
                throw nvae;
            }
        }

        public int Predict(IIntStream input)
        {
            int num5;
            int marker = input.Mark();
            int index = 0;
            try
            {
                int num3;
            Label_0009:
                num3 = this.special[index];
                if (num3 >= 0)
                {
                    index = this.specialStateTransitionHandler(this, num3, input);
                    if (index == -1)
                    {
                        this.NoViableAlt(index, input);
                        return 0;
                    }
                    input.Consume();
                    goto Label_0009;
                }
                if (this.accept[index] >= 1)
                {
                    return this.accept[index];
                }
                char ch = (char) input.LA(1);
                if ((ch >= this.min[index]) && (ch <= this.max[index]))
                {
                    int num4 = this.transition[index][ch - this.min[index]];
                    if (num4 < 0)
                    {
                        if (this.eot[index] < 0)
                        {
                            this.NoViableAlt(index, input);
                            return 0;
                        }
                        index = this.eot[index];
                        input.Consume();
                    }
                    else
                    {
                        index = num4;
                        input.Consume();
                    }
                    goto Label_0009;
                }
                if (this.eot[index] >= 0)
                {
                    index = this.eot[index];
                    input.Consume();
                    goto Label_0009;
                }
                if ((ch == ((char) Token.EOF)) && (this.eof[index] >= 0))
                {
                    return this.accept[this.eof[index]];
                }
                this.NoViableAlt(index, input);
                num5 = 0;
            }
            finally
            {
                input.Rewind(marker);
            }
            return num5;
        }

        public virtual int SpecialStateTransition(int s, IIntStream input)
        {
            return -1;
        }

        public int SpecialTransition(int state, int symbol)
        {
            return 0;
        }

        public static short[] UnpackEncodedString(string encodedString)
        {
            int num = 0;
            for (int i = 0; i < encodedString.Length; i += 2)
            {
                num += encodedString[i];
            }
            short[] numArray = new short[num];
            int num3 = 0;
            for (int j = 0; j < encodedString.Length; j += 2)
            {
                char ch = encodedString[j];
                char ch2 = encodedString[j + 1];
                for (int k = 1; k <= ch; k++)
                {
                    numArray[num3++] = (short) ch2;
                }
            }
            return numArray;
        }

        public static short[][] UnpackEncodedStringArray(string[] encodedStrings)
        {
            short[][] numArray = new short[encodedStrings.Length][];
            for (int i = 0; i < encodedStrings.Length; i++)
            {
                numArray[i] = UnpackEncodedString(encodedStrings[i]);
            }
            return numArray;
        }

        public static char[] UnpackEncodedStringToUnsignedChars(string encodedString)
        {
            int num = 0;
            for (int i = 0; i < encodedString.Length; i += 2)
            {
                num += encodedString[i];
            }
            char[] chArray = new char[num];
            int num3 = 0;
            for (int j = 0; j < encodedString.Length; j += 2)
            {
                char ch = encodedString[j];
                char ch2 = encodedString[j + 1];
                for (int k = 1; k <= ch; k++)
                {
                    chArray[num3++] = ch2;
                }
            }
            return chArray;
        }

        public virtual string Description
        {
            get
            {
                return "n/a";
            }
        }

        public delegate int SpecialStateTransitionHandler(DFA dfa, int s, IIntStream input);
    }
}

