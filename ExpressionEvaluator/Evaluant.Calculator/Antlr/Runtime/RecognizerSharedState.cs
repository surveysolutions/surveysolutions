namespace Antlr.Runtime
{
    using System;
    using System.Collections;

    public class RecognizerSharedState
    {
        public int backtracking;
        public int channel;
        public bool errorRecovery;
        public bool failed;
        public BitSet[] following = new BitSet[100];
        public int followingStackPointer = -1;
        public int lastErrorIndex = -1;
        public IDictionary[] ruleMemo;
        public int syntaxErrors;
        public string text;
        public IToken token;
        public int tokenStartCharIndex = -1;
        public int tokenStartCharPositionInLine;
        public int tokenStartLine;
        public int type;
    }
}

