namespace Antlr.Runtime
{
    using System;

    public abstract class Lexer : BaseRecognizer, ITokenSource
    {
        protected internal ICharStream input;
        private const int TOKEN_dot_EOF = -1;

        public Lexer()
        {
        }

        public Lexer(ICharStream input)
        {
            this.input = input;
        }

        public Lexer(ICharStream input, RecognizerSharedState state) : base(state)
        {
            this.input = input;
        }

        public virtual IToken Emit()
        {
            IToken token = new CommonToken(this.input, base.state.type, base.state.channel, base.state.tokenStartCharIndex, this.CharIndex - 1) {
                Line = base.state.tokenStartLine,
                Text = base.state.text,
                CharPositionInLine = base.state.tokenStartCharPositionInLine
            };
            this.Emit(token);
            return token;
        }

        public virtual void Emit(IToken token)
        {
            base.state.token = token;
        }

        public string GetCharErrorDisplay(int c)
        {
            string str;
            switch (c)
            {
                case 9:
                    str = @"\t";
                    break;

                case 10:
                    str = @"\n";
                    break;

                case 13:
                    str = @"\r";
                    break;

                case -1:
                    str = "<EOF>";
                    break;

                default:
                    str = Convert.ToString((char) c);
                    break;
            }
            return ("'" + str + "'");
        }

        public override string GetErrorMessage(RecognitionException e, string[] tokenNames)
        {
            if (e is MismatchedTokenException)
            {
                MismatchedTokenException exception = (MismatchedTokenException) e;
                return ("mismatched character " + this.GetCharErrorDisplay(e.Char) + " expecting " + this.GetCharErrorDisplay(exception.Expecting));
            }
            if (e is NoViableAltException)
            {
                NoViableAltException exception2 = (NoViableAltException) e;
                return ("no viable alternative at character " + this.GetCharErrorDisplay(exception2.Char));
            }
            if (e is EarlyExitException)
            {
                EarlyExitException exception3 = (EarlyExitException) e;
                return ("required (...)+ loop did not match anything at character " + this.GetCharErrorDisplay(exception3.Char));
            }
            if (e is MismatchedNotSetException)
            {
                MismatchedSetException exception4 = (MismatchedSetException) e;
                return string.Concat(new object[] { "mismatched character ", this.GetCharErrorDisplay(exception4.Char), " expecting set ", exception4.expecting });
            }
            if (e is MismatchedSetException)
            {
                MismatchedSetException exception5 = (MismatchedSetException) e;
                return string.Concat(new object[] { "mismatched character ", this.GetCharErrorDisplay(exception5.Char), " expecting set ", exception5.expecting });
            }
            if (e is MismatchedRangeException)
            {
                MismatchedRangeException exception6 = (MismatchedRangeException) e;
                return ("mismatched character " + this.GetCharErrorDisplay(exception6.Char) + " expecting set " + this.GetCharErrorDisplay(exception6.A) + ".." + this.GetCharErrorDisplay(exception6.B));
            }
            return base.GetErrorMessage(e, tokenNames);
        }

        public virtual void Match(int c)
        {
            if (this.input.LA(1) != c)
            {
                if (base.state.backtracking > 0)
                {
                    base.state.failed = true;
                    return;
                }
                MismatchedTokenException re = new MismatchedTokenException(c, this.input);
                this.Recover(re);
                throw re;
            }
            this.input.Consume();
            base.state.failed = false;
        }

        public virtual void Match(string s)
        {
            int num = 0;
            while (num < s.Length)
            {
                if (this.input.LA(1) != s[num])
                {
                    if (base.state.backtracking > 0)
                    {
                        base.state.failed = true;
                        return;
                    }
                    MismatchedTokenException re = new MismatchedTokenException(s[num], this.input);
                    this.Recover(re);
                    throw re;
                }
                num++;
                this.input.Consume();
                base.state.failed = false;
            }
        }

        public virtual void MatchAny()
        {
            this.input.Consume();
        }

        public virtual void MatchRange(int a, int b)
        {
            if ((this.input.LA(1) < a) || (this.input.LA(1) > b))
            {
                if (base.state.backtracking > 0)
                {
                    base.state.failed = true;
                    return;
                }
                MismatchedRangeException re = new MismatchedRangeException(a, b, this.input);
                this.Recover(re);
                throw re;
            }
            this.input.Consume();
            base.state.failed = false;
        }

        public abstract void mTokens();
        public virtual IToken NextToken()
        {
            IToken token;
        Label_0000:
            base.state.token = null;
            base.state.channel = 0;
            base.state.tokenStartCharIndex = this.input.Index();
            base.state.tokenStartCharPositionInLine = this.input.CharPositionInLine;
            base.state.tokenStartLine = this.input.Line;
            base.state.text = null;
            if (this.input.LA(1) == -1)
            {
                return Token.EOF_TOKEN;
            }
            try
            {
                this.mTokens();
                if (base.state.token == null)
                {
                    this.Emit();
                }
                else if (base.state.token == Token.SKIP_TOKEN)
                {
                    goto Label_0000;
                }
                token = base.state.token;
            }
            catch (NoViableAltException exception)
            {
                this.ReportError(exception);
                this.Recover(exception);
                goto Label_0000;
            }
            catch (RecognitionException exception2)
            {
                this.ReportError(exception2);
                goto Label_0000;
            }
            return token;
        }

        public virtual void Recover(RecognitionException re)
        {
            this.input.Consume();
        }

        public override void ReportError(RecognitionException e)
        {
            this.DisplayRecognitionError(this.TokenNames, e);
        }

        public override void Reset()
        {
            base.Reset();
            if (this.input != null)
            {
                this.input.Seek(0);
            }
            if (base.state != null)
            {
                base.state.token = null;
                base.state.type = 0;
                base.state.channel = 0;
                base.state.tokenStartCharIndex = -1;
                base.state.tokenStartCharPositionInLine = -1;
                base.state.tokenStartLine = -1;
                base.state.text = null;
            }
        }

        public void Skip()
        {
            base.state.token = Token.SKIP_TOKEN;
        }

        public virtual void TraceIn(string ruleName, int ruleIndex)
        {
            string inputSymbol = string.Concat(new object[] { (char) this.input.LT(1), " line=", this.Line, ":", this.CharPositionInLine });
            base.TraceIn(ruleName, ruleIndex, inputSymbol);
        }

        public virtual void TraceOut(string ruleName, int ruleIndex)
        {
            string inputSymbol = string.Concat(new object[] { (char) this.input.LT(1), " line=", this.Line, ":", this.CharPositionInLine });
            base.TraceOut(ruleName, ruleIndex, inputSymbol);
        }

        public virtual int CharIndex
        {
            get
            {
                return this.input.Index();
            }
        }

        public virtual int CharPositionInLine
        {
            get
            {
                return this.input.CharPositionInLine;
            }
        }

        public virtual ICharStream CharStream
        {
            get
            {
                return this.input;
            }
            set
            {
                this.input = null;
                this.Reset();
                this.input = value;
            }
        }

        public override IIntStream Input
        {
            get
            {
                return this.input;
            }
        }

        public virtual int Line
        {
            get
            {
                return this.input.Line;
            }
        }

        public override string SourceName
        {
            get
            {
                return this.input.SourceName;
            }
        }

        public virtual string Text
        {
            get
            {
                if (base.state.text != null)
                {
                    return base.state.text;
                }
                return this.input.Substring(base.state.tokenStartCharIndex, this.CharIndex - 1);
            }
            set
            {
                base.state.text = value;
            }
        }
    }
}

