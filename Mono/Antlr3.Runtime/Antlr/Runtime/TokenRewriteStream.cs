namespace Antlr.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    public class TokenRewriteStream : CommonTokenStream
    {
        public const string DEFAULT_PROGRAM_NAME = "default";
        protected IDictionary lastRewriteTokenIndexes;
        public const int MIN_TOKEN_INDEX = 0;
        public const int PROGRAM_INIT_SIZE = 100;
        protected IDictionary programs;

        public TokenRewriteStream()
        {
            this.Init();
        }

        public TokenRewriteStream(ITokenSource tokenSource) : base(tokenSource)
        {
            this.Init();
        }

        public TokenRewriteStream(ITokenSource tokenSource, int channel) : base(tokenSource, channel)
        {
            this.Init();
        }

        protected string CatOpText(object a, object b)
        {
            string str = "";
            string str2 = "";
            if (a != null)
            {
                str = a.ToString();
            }
            if (b != null)
            {
                str2 = b.ToString();
            }
            return (str + str2);
        }

        public virtual void Delete(IToken indexT)
        {
            this.Delete("default", indexT, indexT);
        }

        public virtual void Delete(int index)
        {
            this.Delete("default", index, index);
        }

        public virtual void Delete(IToken from, IToken to)
        {
            this.Delete("default", from, to);
        }

        public virtual void Delete(int from, int to)
        {
            this.Delete("default", from, to);
        }

        public virtual void Delete(string programName, IToken from, IToken to)
        {
            this.Replace(programName, from, to, null);
        }

        public virtual void Delete(string programName, int from, int to)
        {
            this.Replace(programName, from, to, null);
        }

        public virtual void DeleteProgram()
        {
            this.DeleteProgram("default");
        }

        public virtual void DeleteProgram(string programName)
        {
            this.Rollback(programName, 0);
        }

        protected IList GetKindOfOps(IList rewrites, Type kind)
        {
            return this.GetKindOfOps(rewrites, kind, rewrites.Count);
        }

        protected IList GetKindOfOps(IList rewrites, Type kind, int before)
        {
            IList list = new List<object>();
            for (int i = 0; (i < before) && (i < rewrites.Count); i++)
            {
                RewriteOperation operation = (RewriteOperation) rewrites[i];
                if ((operation != null) && (operation.GetType() == kind))
                {
                    list.Add(operation);
                }
            }
            return list;
        }

        public virtual int GetLastRewriteTokenIndex()
        {
            return this.GetLastRewriteTokenIndex("default");
        }

        protected virtual int GetLastRewriteTokenIndex(string programName)
        {
            object obj2 = this.lastRewriteTokenIndexes[programName];
            if (obj2 == null)
            {
                return -1;
            }
            return (int) obj2;
        }

        protected virtual IList GetProgram(string name)
        {
            IList list = (IList) this.programs[name];
            if (list == null)
            {
                list = this.InitializeProgram(name);
            }
            return list;
        }

        protected internal virtual void Init()
        {
            this.programs = new Hashtable();
            this.programs["default"] = new List<object>(100);
            this.lastRewriteTokenIndexes = new Hashtable();
        }

        private IList InitializeProgram(string name)
        {
            IList list = new List<object>(100);
            this.programs[name] = list;
            return list;
        }

        public virtual void InsertAfter(IToken t, object text)
        {
            this.InsertAfter("default", t, text);
        }

        public virtual void InsertAfter(int index, object text)
        {
            this.InsertAfter("default", index, text);
        }

        public virtual void InsertAfter(string programName, IToken t, object text)
        {
            this.InsertAfter(programName, t.TokenIndex, text);
        }

        public virtual void InsertAfter(string programName, int index, object text)
        {
            this.InsertBefore(programName, (int) (index + 1), text);
        }

        public virtual void InsertBefore(IToken t, object text)
        {
            this.InsertBefore("default", t, text);
        }

        public virtual void InsertBefore(int index, object text)
        {
            this.InsertBefore("default", index, text);
        }

        public virtual void InsertBefore(string programName, IToken t, object text)
        {
            this.InsertBefore(programName, t.TokenIndex, text);
        }

        public virtual void InsertBefore(string programName, int index, object text)
        {
            RewriteOperation operation = new InsertBeforeOp(index, text, this);
            this.GetProgram(programName).Add(operation);
        }

        protected IDictionary ReduceToSingleOperationPerIndex(IList rewrites)
        {
            for (int i = 0; i < rewrites.Count; i++)
            {
                RewriteOperation operation = (RewriteOperation) rewrites[i];
                if ((operation != null) && (operation is ReplaceOp))
                {
                    ReplaceOp op = (ReplaceOp) rewrites[i];
                    IList list = this.GetKindOfOps(rewrites, typeof(InsertBeforeOp), i);
                    for (int m = 0; m < list.Count; m++)
                    {
                        InsertBeforeOp op2 = (InsertBeforeOp) list[m];
                        if ((op2.index >= op.index) && (op2.index <= op.lastIndex))
                        {
                            rewrites[op2.instructionIndex] = null;
                        }
                    }
                    IList list2 = this.GetKindOfOps(rewrites, typeof(ReplaceOp), i);
                    for (int n = 0; n < list2.Count; n++)
                    {
                        ReplaceOp op3 = (ReplaceOp) list2[n];
                        if ((op3.index >= op.index) && (op3.lastIndex <= op.lastIndex))
                        {
                            rewrites[op3.instructionIndex] = null;
                        }
                        else
                        {
                            bool flag = (op3.lastIndex < op.index) || (op3.index > op.lastIndex);
                            bool flag2 = (op3.index == op.index) && (op3.lastIndex == op.lastIndex);
                            if (!flag && !flag2)
                            {
                                throw new ArgumentOutOfRangeException(string.Concat(new object[] { "replace op boundaries of ", op, " overlap with previous ", op3 }));
                            }
                        }
                    }
                }
            }
            for (int j = 0; j < rewrites.Count; j++)
            {
                RewriteOperation operation2 = (RewriteOperation) rewrites[j];
                if ((operation2 != null) && (operation2 is InsertBeforeOp))
                {
                    InsertBeforeOp op4 = (InsertBeforeOp) rewrites[j];
                    IList list3 = this.GetKindOfOps(rewrites, typeof(InsertBeforeOp), j);
                    for (int num5 = 0; num5 < list3.Count; num5++)
                    {
                        InsertBeforeOp op5 = (InsertBeforeOp) list3[num5];
                        if (op5.index == op4.index)
                        {
                            op4.text = this.CatOpText(op4.text, op5.text);
                            rewrites[op5.instructionIndex] = null;
                        }
                    }
                    IList list4 = this.GetKindOfOps(rewrites, typeof(ReplaceOp), j);
                    for (int num6 = 0; num6 < list4.Count; num6++)
                    {
                        ReplaceOp op6 = (ReplaceOp) list4[num6];
                        if (op4.index == op6.index)
                        {
                            op6.text = this.CatOpText(op4.text, op6.text);
                            rewrites[j] = null;
                        }
                        else if ((op4.index >= op6.index) && (op4.index <= op6.lastIndex))
                        {
                            throw new ArgumentOutOfRangeException(string.Concat(new object[] { "insert op ", op4, " within boundaries of previous ", op6 }));
                        }
                    }
                }
            }
            IDictionary dictionary = new Hashtable();
            for (int k = 0; k < rewrites.Count; k++)
            {
                RewriteOperation operation3 = (RewriteOperation) rewrites[k];
                if (operation3 != null)
                {
                    if (dictionary[operation3.index] != null)
                    {
                        throw new Exception("should only be one op per index");
                    }
                    dictionary[operation3.index] = operation3;
                }
            }
            return dictionary;
        }

        public virtual void Replace(IToken indexT, object text)
        {
            this.Replace("default", indexT, indexT, text);
        }

        public virtual void Replace(int index, object text)
        {
            this.Replace("default", index, index, text);
        }

        public virtual void Replace(IToken from, IToken to, object text)
        {
            this.Replace("default", from, to, text);
        }

        public virtual void Replace(int from, int to, object text)
        {
            this.Replace("default", from, to, text);
        }

        public virtual void Replace(string programName, IToken from, IToken to, object text)
        {
            this.Replace(programName, from.TokenIndex, to.TokenIndex, text);
        }

        public virtual void Replace(string programName, int from, int to, object text)
        {
            if (((from > to) || (from < 0)) || ((to < 0) || (to >= base.tokens.Count)))
            {
                throw new ArgumentOutOfRangeException(string.Concat(new object[] { "replace: range invalid: ", from, "..", to, "(size=", base.tokens.Count, ")" }));
            }
            RewriteOperation operation = new ReplaceOp(from, to, text, this);
            IList program = this.GetProgram(programName);
            operation.instructionIndex = program.Count;
            program.Add(operation);
        }

        public virtual void Rollback(int instructionIndex)
        {
            this.Rollback("default", instructionIndex);
        }

        public virtual void Rollback(string programName, int instructionIndex)
        {
            IList list = (IList) this.programs[programName];
            if (list != null)
            {
                this.programs[programName] = ((List<object>) list).GetRange(0, instructionIndex);
            }
        }

        protected virtual void SetLastRewriteTokenIndex(string programName, int i)
        {
            this.lastRewriteTokenIndexes[programName] = i;
        }

        public virtual string ToDebugString()
        {
            return this.ToDebugString(0, this.Count - 1);
        }

        public virtual string ToDebugString(int start, int end)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = start; ((i >= 0) && (i <= end)) && (i < base.tokens.Count); i++)
            {
                builder.Append(this.Get(i));
            }
            return builder.ToString();
        }

        public virtual string ToOriginalString()
        {
            return this.ToOriginalString(0, this.Count - 1);
        }

        public virtual string ToOriginalString(int start, int end)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = start; ((i >= 0) && (i <= end)) && (i < base.tokens.Count); i++)
            {
                builder.Append(this.Get(i).Text);
            }
            return builder.ToString();
        }

        public override string ToString()
        {
            return this.ToString(0, this.Count - 1);
        }

        public virtual string ToString(string programName)
        {
            return this.ToString(programName, 0, this.Count - 1);
        }

        public override string ToString(int start, int end)
        {
            return this.ToString("default", start, end);
        }

        public virtual string ToString(string programName, int start, int end)
        {
            IList rewrites = (IList) this.programs[programName];
            if (end > (base.tokens.Count - 1))
            {
                end = base.tokens.Count - 1;
            }
            if (start < 0)
            {
                start = 0;
            }
            if ((rewrites == null) || (rewrites.Count == 0))
            {
                return this.ToOriginalString(start, end);
            }
            StringBuilder buf = new StringBuilder();
            IDictionary dictionary = this.ReduceToSingleOperationPerIndex(rewrites);
            int key = start;
            while ((key <= end) && (key < base.tokens.Count))
            {
                RewriteOperation operation = (RewriteOperation) dictionary[key];
                dictionary.Remove(key);
                IToken token = (IToken) base.tokens[key];
                if (operation == null)
                {
                    buf.Append(token.Text);
                    key++;
                }
                else
                {
                    key = operation.Execute(buf);
                }
            }
            if (end == (base.tokens.Count - 1))
            {
                IEnumerator enumerator = dictionary.Values.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    InsertBeforeOp current = (InsertBeforeOp) enumerator.Current;
                    if (current.index >= (base.tokens.Count - 1))
                    {
                        buf.Append(current.text);
                    }
                }
            }
            return buf.ToString();
        }

        internal protected class DeleteOp : TokenRewriteStream.ReplaceOp
        {
            public DeleteOp(int from, int to, TokenRewriteStream parent) : base(from, to, null, parent)
            {
            }

            public override string ToString()
            {
                return string.Concat(new object[] { "<DeleteOp@", base.index, "..", base.lastIndex, ">" });
            }
        }

        internal protected class InsertBeforeOp : TokenRewriteStream.RewriteOperation
        {
            public InsertBeforeOp(int index, object text, TokenRewriteStream parent) : base(index, text, parent)
            {
            }

            public override int Execute(StringBuilder buf)
            {
                buf.Append(base.text);
                buf.Append(base.parent.Get(base.index).Text);
                return (base.index + 1);
            }
        }

        internal protected class ReplaceOp : TokenRewriteStream.RewriteOperation
        {
            protected internal int lastIndex;

            public ReplaceOp(int from, int to, object text, TokenRewriteStream parent) : base(from, text, parent)
            {
                this.lastIndex = to;
            }

            public override int Execute(StringBuilder buf)
            {
                if (base.text != null)
                {
                    buf.Append(base.text);
                }
                return (this.lastIndex + 1);
            }

            public override string ToString()
            {
                return string.Concat(new object[] { "<ReplaceOp@", base.index, "..", this.lastIndex, ":\"", base.text, "\">" });
            }
        }

        private class RewriteOpComparer : IComparer
        {
            public virtual int Compare(object o1, object o2)
            {
                TokenRewriteStream.RewriteOperation operation = (TokenRewriteStream.RewriteOperation) o1;
                TokenRewriteStream.RewriteOperation operation2 = (TokenRewriteStream.RewriteOperation) o2;
                if (operation.index < operation2.index)
                {
                    return -1;
                }
                if (operation.index > operation2.index)
                {
                    return 1;
                }
                return 0;
            }
        }

        internal protected class RewriteOperation
        {
            protected internal int index;
            protected internal int instructionIndex;
            protected internal TokenRewriteStream parent;
            protected internal object text;

            protected internal RewriteOperation(int index, object text, TokenRewriteStream parent)
            {
                this.index = index;
                this.text = text;
                this.parent = parent;
            }

            public virtual int Execute(StringBuilder buf)
            {
                return this.index;
            }

            public override string ToString()
            {
                string fullName = base.GetType().FullName;
                int index = fullName.IndexOf('$');
                fullName = fullName.Substring(index + 1, fullName.Length - (index + 1));
                return string.Concat(new object[] { "<", fullName, "@", this.index, ":\"", this.text, "\">" });
            }
        }
    }
}

