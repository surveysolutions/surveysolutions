namespace Antlr.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    public abstract class BaseRecognizer
    {
        public const int DEFAULT_TOKEN_CHANNEL = 0;
        public const int HIDDEN = 0x63;
        public const int INITIAL_FOLLOW_STACK_SIZE = 100;
        public const int MEMO_RULE_FAILED = -2;
        public const int MEMO_RULE_UNKNOWN = -1;
        public static readonly string NEXT_TOKEN_RULE_NAME = "nextToken";
        protected internal RecognizerSharedState state;

        public BaseRecognizer()
        {
            this.state = new RecognizerSharedState();
        }

        public BaseRecognizer(RecognizerSharedState state)
        {
            if (state == null)
            {
                state = new RecognizerSharedState();
            }
            this.state = state;
        }

        public virtual bool AlreadyParsedRule(IIntStream input, int ruleIndex)
        {
            int ruleMemoization = this.GetRuleMemoization(ruleIndex, input.Index());
            switch (ruleMemoization)
            {
                case -1:
                    return false;

                case -2:
                    this.state.failed = true;
                    break;

                default:
                    input.Seek(ruleMemoization + 1);
                    break;
            }
            return true;
        }

        public virtual void BeginBacktrack(int level)
        {
        }

        public virtual void BeginResync()
        {
        }

        protected internal virtual BitSet CombineFollows(bool exact)
        {
            int followingStackPointer = this.state.followingStackPointer;
            BitSet set = new BitSet();
            for (int i = followingStackPointer; i >= 0; i--)
            {
                BitSet a = this.state.following[i];
                set.OrInPlace(a);
                if (exact)
                {
                    if (!a.Member(1))
                    {
                        return set;
                    }
                    if (i > 0)
                    {
                        set.Remove(1);
                    }
                }
            }
            return set;
        }

        protected internal virtual BitSet ComputeContextSensitiveRuleFOLLOW()
        {
            return this.CombineFollows(true);
        }

        protected internal virtual BitSet ComputeErrorRecoverySet()
        {
            return this.CombineFollows(false);
        }

        public virtual void ConsumeUntil(IIntStream input, BitSet set)
        {
            for (int i = input.LA(1); (i != Token.EOF) && !set.Member(i); i = input.LA(1))
            {
                input.Consume();
            }
        }

        public virtual void ConsumeUntil(IIntStream input, int tokenType)
        {
            for (int i = input.LA(1); (i != Token.EOF) && (i != tokenType); i = input.LA(1))
            {
                input.Consume();
            }
        }

        public virtual void DisplayRecognitionError(string[] tokenNames, RecognitionException e)
        {
            this.EmitErrorMessage(this.GetErrorHeader(e) + " " + this.GetErrorMessage(e, tokenNames));
        }

        public virtual void EmitErrorMessage(string msg)
        {
            Console.Error.WriteLine(msg);
        }

        public virtual void EndBacktrack(int level, bool successful)
        {
        }

        public virtual void EndResync()
        {
        }

        public bool Failed()
        {
            return this.state.failed;
        }

        protected virtual object GetCurrentInputSymbol(IIntStream input)
        {
            return null;
        }

        public virtual string GetErrorHeader(RecognitionException e)
        {
            return string.Concat(new object[] { "line ", e.Line, ":", e.CharPositionInLine });
        }

        public virtual string GetErrorMessage(RecognitionException e, string[] tokenNames)
        {
            string message = e.Message;
            if (e is UnwantedTokenException)
            {
                UnwantedTokenException exception = (UnwantedTokenException) e;
                string str2 = "<unknown>";
                if (exception.Expecting == Token.EOF)
                {
                    str2 = "EOF";
                }
                else
                {
                    str2 = tokenNames[exception.Expecting];
                }
                return ("extraneous input " + this.GetTokenErrorDisplay(exception.UnexpectedToken) + " expecting " + str2);
            }
            if (e is MissingTokenException)
            {
                MissingTokenException exception2 = (MissingTokenException) e;
                string str3 = "<unknown>";
                if (exception2.Expecting == Token.EOF)
                {
                    str3 = "EOF";
                }
                else
                {
                    str3 = tokenNames[exception2.Expecting];
                }
                return ("missing " + str3 + " at " + this.GetTokenErrorDisplay(e.Token));
            }
            if (e is MismatchedTokenException)
            {
                MismatchedTokenException exception3 = (MismatchedTokenException) e;
                string str4 = "<unknown>";
                if (exception3.Expecting == Token.EOF)
                {
                    str4 = "EOF";
                }
                else
                {
                    str4 = tokenNames[exception3.Expecting];
                }
                return ("mismatched input " + this.GetTokenErrorDisplay(e.Token) + " expecting " + str4);
            }
            if (e is MismatchedTreeNodeException)
            {
                MismatchedTreeNodeException exception4 = (MismatchedTreeNodeException) e;
                string str5 = "<unknown>";
                if (exception4.expecting == Token.EOF)
                {
                    str5 = "EOF";
                }
                else
                {
                    str5 = tokenNames[exception4.expecting];
                }
                object[] objArray = new object[] { "mismatched tree node: ", ((exception4.Node != null) && (exception4.Node.ToString() != null)) ? exception4.Node : string.Empty, " expecting ", str5 };
                return string.Concat(objArray);
            }
            if (e is NoViableAltException)
            {
                return ("no viable alternative at input " + this.GetTokenErrorDisplay(e.Token));
            }
            if (e is EarlyExitException)
            {
                return ("required (...)+ loop did not match anything at input " + this.GetTokenErrorDisplay(e.Token));
            }
            if (e is MismatchedSetException)
            {
                MismatchedSetException exception5 = (MismatchedSetException) e;
                return string.Concat(new object[] { "mismatched input ", this.GetTokenErrorDisplay(e.Token), " expecting set ", exception5.expecting });
            }
            if (e is MismatchedNotSetException)
            {
                MismatchedNotSetException exception6 = (MismatchedNotSetException) e;
                return string.Concat(new object[] { "mismatched input ", this.GetTokenErrorDisplay(e.Token), " expecting set ", exception6.expecting });
            }
            if (e is FailedPredicateException)
            {
                FailedPredicateException exception7 = (FailedPredicateException) e;
                message = "rule " + exception7.ruleName + " failed predicate: {" + exception7.predicateText + "}?";
            }
            return message;
        }

        protected virtual object GetMissingSymbol(IIntStream input, RecognitionException e, int expectedTokenType, BitSet follow)
        {
            return null;
        }

        public virtual IList GetRuleInvocationStack()
        {
            string fullName = base.GetType().FullName;
            return GetRuleInvocationStack(new Exception(), fullName);
        }

        public static IList GetRuleInvocationStack(Exception e, string recognizerClassName)
        {
            IList list = new List<object>();
            StackTrace trace = new StackTrace(e);
            int index = 0;
            for (index = trace.FrameCount - 1; index >= 0; index--)
            {
                StackFrame frame = trace.GetFrame(index);
                if ((!frame.GetMethod().DeclaringType.FullName.StartsWith("Antlr.Runtime.") && !frame.GetMethod().Name.Equals(NEXT_TOKEN_RULE_NAME)) && frame.GetMethod().DeclaringType.FullName.Equals(recognizerClassName))
                {
                    list.Add(frame.GetMethod().Name);
                }
            }
            return list;
        }

        public virtual int GetRuleMemoization(int ruleIndex, int ruleStartIndex)
        {
            if (this.state.ruleMemo[ruleIndex] == null)
            {
                this.state.ruleMemo[ruleIndex] = new Hashtable();
            }
            object obj2 = this.state.ruleMemo[ruleIndex][ruleStartIndex];
            if (obj2 == null)
            {
                return -1;
            }
            return (int) obj2;
        }

        public int GetRuleMemoizationCacheSize()
        {
            int num = 0;
            for (int i = 0; (this.state.ruleMemo != null) && (i < this.state.ruleMemo.Length); i++)
            {
                IDictionary dictionary = this.state.ruleMemo[i];
                if (dictionary != null)
                {
                    num += dictionary.Count;
                }
            }
            return num;
        }

        public virtual string GetTokenErrorDisplay(IToken t)
        {
            string text = t.Text;
            if (text == null)
            {
                if (t.Type == Token.EOF)
                {
                    text = "<EOF>";
                }
                else
                {
                    text = "<" + t.Type + ">";
                }
            }
            text = text.Replace("\n", @"\\n").Replace("\r", @"\\r").Replace("\t", @"\\t");
            return ("'" + text + "'");
        }

        public virtual object Match(IIntStream input, int ttype, BitSet follow)
        {
            object currentInputSymbol = this.GetCurrentInputSymbol(input);
            if (input.LA(1) == ttype)
            {
                input.Consume();
                this.state.errorRecovery = false;
                this.state.failed = false;
                return currentInputSymbol;
            }
            if (this.state.backtracking > 0)
            {
                this.state.failed = true;
                return currentInputSymbol;
            }
            return this.RecoverFromMismatchedToken(input, ttype, follow);
        }

        public virtual void MatchAny(IIntStream input)
        {
            this.state.errorRecovery = false;
            this.state.failed = false;
            input.Consume();
        }

        public virtual void Memoize(IIntStream input, int ruleIndex, int ruleStartIndex)
        {
            int num = this.state.failed ? -2 : (input.Index() - 1);
            if (this.state.ruleMemo[ruleIndex] != null)
            {
                this.state.ruleMemo[ruleIndex][ruleStartIndex] = num;
            }
        }

        public bool MismatchIsMissingToken(IIntStream input, BitSet follow)
        {
            if (follow == null)
            {
                return false;
            }
            if (follow.Member(1))
            {
                BitSet a = this.ComputeContextSensitiveRuleFOLLOW();
                follow = follow.Or(a);
                if (this.state.followingStackPointer >= 0)
                {
                    follow.Remove(1);
                }
            }
            if (!follow.Member(input.LA(1)) && !follow.Member(1))
            {
                return false;
            }
            return true;
        }

        public bool MismatchIsUnwantedToken(IIntStream input, int ttype)
        {
            return (input.LA(2) == ttype);
        }

        protected void PushFollow(BitSet fset)
        {
            if ((this.state.followingStackPointer + 1) >= this.state.following.Length)
            {
                BitSet[] destinationArray = new BitSet[this.state.following.Length * 2];
                Array.Copy(this.state.following, 0, destinationArray, 0, this.state.following.Length);
                this.state.following = destinationArray;
            }
            this.state.following[++this.state.followingStackPointer] = fset;
        }

        public virtual void Recover(IIntStream input, RecognitionException re)
        {
            if (this.state.lastErrorIndex == input.Index())
            {
                input.Consume();
            }
            this.state.lastErrorIndex = input.Index();
            BitSet set = this.ComputeErrorRecoverySet();
            this.BeginResync();
            this.ConsumeUntil(input, set);
            this.EndResync();
        }

        public virtual object RecoverFromMismatchedSet(IIntStream input, RecognitionException e, BitSet follow)
        {
            if (!this.MismatchIsMissingToken(input, follow))
            {
                throw e;
            }
            this.ReportError(e);
            return this.GetMissingSymbol(input, e, 0, follow);
        }

        protected internal virtual object RecoverFromMismatchedToken(IIntStream input, int ttype, BitSet follow)
        {
            RecognitionException e = null;
            if (this.MismatchIsUnwantedToken(input, ttype))
            {
                e = new UnwantedTokenException(ttype, input);
                this.BeginResync();
                input.Consume();
                this.EndResync();
                this.ReportError(e);
                object currentInputSymbol = this.GetCurrentInputSymbol(input);
                input.Consume();
                return currentInputSymbol;
            }
            if (this.MismatchIsMissingToken(input, follow))
            {
                object inserted = this.GetMissingSymbol(input, e, ttype, follow);
                e = new MissingTokenException(ttype, input, inserted);
                this.ReportError(e);
                return inserted;
            }
            e = new MismatchedTokenException(ttype, input);
            throw e;
        }

        public virtual void ReportError(RecognitionException e)
        {
            if (!this.state.errorRecovery)
            {
                this.state.syntaxErrors++;
                this.state.errorRecovery = true;
                this.DisplayRecognitionError(this.TokenNames, e);
            }
        }

        public virtual void Reset()
        {
            if (this.state != null)
            {
                this.state.followingStackPointer = -1;
                this.state.errorRecovery = false;
                this.state.lastErrorIndex = -1;
                this.state.failed = false;
                this.state.syntaxErrors = 0;
                this.state.backtracking = 0;
                for (int i = 0; (this.state.ruleMemo != null) && (i < this.state.ruleMemo.Length); i++)
                {
                    this.state.ruleMemo[i] = null;
                }
            }
        }

        public virtual IList ToStrings(IList tokens)
        {
            if (tokens == null)
            {
                return null;
            }
            IList list = new List<object>(tokens.Count);
            for (int i = 0; i < tokens.Count; i++)
            {
                list.Add(((IToken) tokens[i]).Text);
            }
            return list;
        }

        public virtual void TraceIn(string ruleName, int ruleIndex, object inputSymbol)
        {
            Console.Out.Write(string.Concat(new object[] { "enter ", ruleName, " ", inputSymbol }));
            if (this.state.backtracking > 0)
            {
                Console.Out.Write(" backtracking=" + this.state.backtracking);
            }
            Console.Out.WriteLine();
        }

        public virtual void TraceOut(string ruleName, int ruleIndex, object inputSymbol)
        {
            Console.Out.Write(string.Concat(new object[] { "exit ", ruleName, " ", inputSymbol }));
            if (this.state.backtracking > 0)
            {
                Console.Out.Write(" backtracking=" + this.state.backtracking);
                if (this.state.failed)
                {
                    Console.Out.WriteLine(" failed" + this.state.failed);
                }
                else
                {
                    Console.Out.WriteLine(" succeeded" + this.state.failed);
                }
            }
            Console.Out.WriteLine();
        }

        public int BacktrackingLevel
        {
            get
            {
                return this.state.backtracking;
            }
            set
            {
                this.state.backtracking = value;
            }
        }

        public virtual string GrammarFileName
        {
            get
            {
                return null;
            }
        }

        public abstract IIntStream Input { get; }

        public int NumberOfSyntaxErrors
        {
            get
            {
                return this.state.syntaxErrors;
            }
        }

        public abstract string SourceName { get; }

        public virtual string[] TokenNames
        {
            get
            {
                return null;
            }
        }
    }
}

