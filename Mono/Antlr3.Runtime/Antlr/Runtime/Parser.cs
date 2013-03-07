namespace Antlr.Runtime
{
    using System;

    public class Parser : BaseRecognizer
    {
        protected internal ITokenStream input;

        public Parser(ITokenStream input)
        {
            this.TokenStream = input;
        }

        public Parser(ITokenStream input, RecognizerSharedState state) : base(state)
        {
            this.TokenStream = input;
        }

        protected override object GetCurrentInputSymbol(IIntStream input)
        {
            return ((ITokenStream) input).LT(1);
        }

        protected override object GetMissingSymbol(IIntStream input, RecognitionException e, int expectedTokenType, BitSet follow)
        {
            string text = null;
            if (expectedTokenType == Token.EOF)
            {
                text = "<missing EOF>";
            }
            else
            {
                text = "<missing " + this.TokenNames[expectedTokenType] + ">";
            }
            CommonToken token = new CommonToken(expectedTokenType, text);
            IToken token2 = ((ITokenStream) input).LT(1);
            if (token2.Type == Token.EOF)
            {
                token2 = ((ITokenStream) input).LT(-1);
            }
            token.line = token2.Line;
            token.CharPositionInLine = token2.CharPositionInLine;
            token.Channel = 0;
            return token;
        }

        public override void Reset()
        {
            base.Reset();
            if (this.input != null)
            {
                this.input.Seek(0);
            }
        }

        public virtual void TraceIn(string ruleName, int ruleIndex)
        {
            base.TraceIn(ruleName, ruleIndex, this.input.LT(1));
        }

        public virtual void TraceOut(string ruleName, int ruleIndex)
        {
            base.TraceOut(ruleName, ruleIndex, this.input.LT(1));
        }

        public override IIntStream Input
        {
            get
            {
                return this.input;
            }
        }

        public override string SourceName
        {
            get
            {
                return this.input.SourceName;
            }
        }

        public virtual ITokenStream TokenStream
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
    }
}

