namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using System;
    using System.Text.RegularExpressions;

    public class TreeParser : BaseRecognizer
    {
        private static readonly string dotdot = @".*[^.]\.\.[^.].*";
        private static readonly Regex dotdotPattern = new Regex(dotdot, RegexOptions.Compiled);
        private static readonly string doubleEtc = @".*\.\.\.\s+\.\.\..*";
        private static readonly Regex doubleEtcPattern = new Regex(doubleEtc, RegexOptions.Compiled);
        public const int DOWN = 2;
        protected internal ITreeNodeStream input;
        private static readonly string spaces = @"\s+";
        private static readonly Regex spacesPattern = new Regex(spaces, RegexOptions.Compiled);
        public const int UP = 3;

        public TreeParser(ITreeNodeStream input)
        {
            this.TreeNodeStream = input;
        }

        public TreeParser(ITreeNodeStream input, RecognizerSharedState state) : base(state)
        {
            this.TreeNodeStream = input;
        }

        protected override object GetCurrentInputSymbol(IIntStream input)
        {
            return ((ITreeNodeStream) input).LT(1);
        }

        public override string GetErrorHeader(RecognitionException e)
        {
            return string.Concat(new object[] { this.GrammarFileName, ": node from ", e.approximateLineInfo ? "after " : "", "line ", e.Line, ":", e.CharPositionInLine });
        }

        public override string GetErrorMessage(RecognitionException e, string[] tokenNames)
        {
            if (this != null)
            {
                ITreeAdaptor treeAdaptor = ((ITreeNodeStream) e.Input).TreeAdaptor;
                e.Token = treeAdaptor.GetToken(e.Node);
                if (e.Token == null)
                {
                    e.Token = new CommonToken(treeAdaptor.GetNodeType(e.Node), treeAdaptor.GetNodeText(e.Node));
                }
            }
            return base.GetErrorMessage(e, tokenNames);
        }

        protected override object GetMissingSymbol(IIntStream input, RecognitionException e, int expectedTokenType, BitSet follow)
        {
            string text = "<missing " + this.TokenNames[expectedTokenType] + ">";
            return new CommonTree(new CommonToken(expectedTokenType, text));
        }

        public override void MatchAny(IIntStream ignore)
        {
            base.state.errorRecovery = false;
            base.state.failed = false;
            object t = this.input.LT(1);
            if (this.input.TreeAdaptor.GetChildCount(t) == 0)
            {
                this.input.Consume();
            }
            else
            {
                int num = 0;
                int nodeType = this.input.TreeAdaptor.GetNodeType(t);
                while ((nodeType != Token.EOF) && ((nodeType != 3) || (num != 0)))
                {
                    this.input.Consume();
                    t = this.input.LT(1);
                    switch (this.input.TreeAdaptor.GetNodeType(t))
                    {
                        case 2:
                        {
                            num++;
                            continue;
                        }
                        case 3:
                            num--;
                            break;
                    }
                }
                this.input.Consume();
            }
        }

        protected internal override object RecoverFromMismatchedToken(IIntStream input, int ttype, BitSet follow)
        {
            throw new MismatchedTreeNodeException(ttype, (ITreeNodeStream) input);
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

        public virtual ITreeNodeStream TreeNodeStream
        {
            get
            {
                return this.input;
            }
            set
            {
                this.input = value;
            }
        }
    }
}

