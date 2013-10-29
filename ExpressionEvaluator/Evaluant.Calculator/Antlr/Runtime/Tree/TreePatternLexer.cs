namespace Antlr.Runtime.Tree
{
    using System;
    using System.Text;

    public class TreePatternLexer
    {
        public const int ARG = 4;
        public const int BEGIN = 1;
        protected int c;
        public const int COLON = 6;
        public const int DOT = 7;
        public const int END = 2;
        public const int EOF = -1;
        public bool error;
        public const int ID = 3;
        protected int n;
        protected int p = -1;
        protected string pattern;
        public const int PERCENT = 5;
        public StringBuilder sval = new StringBuilder();

        public TreePatternLexer(string pattern)
        {
            this.pattern = pattern;
            this.n = pattern.Length;
            this.Consume();
        }

        protected void Consume()
        {
            this.p++;
            if (this.p >= this.n)
            {
                this.c = -1;
            }
            else
            {
                this.c = this.pattern[this.p];
            }
        }

        public int NextToken()
        {
            this.sval.Length = 0;
            while (this.c != -1)
            {
                if (((this.c == 0x20) || (this.c == 10)) || ((this.c == 13) || (this.c == 9)))
                {
                    this.Consume();
                }
                else
                {
                    if ((((this.c >= 0x61) && (this.c <= 0x7a)) || ((this.c >= 0x41) && (this.c <= 90))) || (this.c == 0x5f))
                    {
                        this.sval.Append((char) this.c);
                        this.Consume();
                        while ((((this.c >= 0x61) && (this.c <= 0x7a)) || ((this.c >= 0x41) && (this.c <= 90))) || (((this.c >= 0x30) && (this.c <= 0x39)) || (this.c == 0x5f)))
                        {
                            this.sval.Append((char) this.c);
                            this.Consume();
                        }
                        return 3;
                    }
                    if (this.c == 40)
                    {
                        this.Consume();
                        return 1;
                    }
                    if (this.c == 0x29)
                    {
                        this.Consume();
                        return 2;
                    }
                    if (this.c == 0x25)
                    {
                        this.Consume();
                        return 5;
                    }
                    if (this.c == 0x3a)
                    {
                        this.Consume();
                        return 6;
                    }
                    if (this.c == 0x2e)
                    {
                        this.Consume();
                        return 7;
                    }
                    if (this.c == 0x5b)
                    {
                        this.Consume();
                        while (this.c != 0x5d)
                        {
                            if (this.c == 0x5c)
                            {
                                this.Consume();
                                if (this.c != 0x5d)
                                {
                                    this.sval.Append('\\');
                                }
                                this.sval.Append((char) this.c);
                            }
                            else
                            {
                                this.sval.Append((char) this.c);
                            }
                            this.Consume();
                        }
                        this.Consume();
                        return 4;
                    }
                    this.Consume();
                    this.error = true;
                    return -1;
                }
            }
            return -1;
        }
    }
}

