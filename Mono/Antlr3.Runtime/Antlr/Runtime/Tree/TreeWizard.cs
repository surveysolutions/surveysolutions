namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class TreeWizard
    {
        protected ITreeAdaptor adaptor;
        protected IDictionary tokenNameToTypeMap;

        public TreeWizard(ITreeAdaptor adaptor)
        {
            this.adaptor = adaptor;
        }

        public TreeWizard(string[] tokenNames) : this(null, tokenNames)
        {
        }

        public TreeWizard(ITreeAdaptor adaptor, IDictionary tokenNameToTypeMap)
        {
            this.adaptor = adaptor;
            this.tokenNameToTypeMap = tokenNameToTypeMap;
        }

        public TreeWizard(ITreeAdaptor adaptor, string[] tokenNames)
        {
            this.adaptor = adaptor;
            this.tokenNameToTypeMap = this.ComputeTokenTypes(tokenNames);
        }

        protected static bool _Equals(object t1, object t2, ITreeAdaptor adaptor)
        {
            if ((t1 == null) || (t2 == null))
            {
                return false;
            }
            if (adaptor.GetNodeType(t1) != adaptor.GetNodeType(t2))
            {
                return false;
            }
            if (!adaptor.GetNodeText(t1).Equals(adaptor.GetNodeText(t2)))
            {
                return false;
            }
            int childCount = adaptor.GetChildCount(t1);
            int num2 = adaptor.GetChildCount(t2);
            if (childCount != num2)
            {
                return false;
            }
            for (int i = 0; i < childCount; i++)
            {
                object child = adaptor.GetChild(t1, i);
                object obj3 = adaptor.GetChild(t2, i);
                if (!_Equals(child, obj3, adaptor))
                {
                    return false;
                }
            }
            return true;
        }

        protected void _Index(object t, IDictionary m)
        {
            if (t != null)
            {
                int nodeType = this.adaptor.GetNodeType(t);
                IList list = m[nodeType] as IList;
                if (list == null)
                {
                    list = new List<object>();
                    m[nodeType] = list;
                }
                list.Add(t);
                int childCount = this.adaptor.GetChildCount(t);
                for (int i = 0; i < childCount; i++)
                {
                    object child = this.adaptor.GetChild(t, i);
                    this._Index(child, m);
                }
            }
        }

        protected bool _Parse(object t1, TreePattern t2, IDictionary labels)
        {
            if ((t1 == null) || (t2 == null))
            {
                return false;
            }
            if (t2.GetType() != typeof(WildcardTreePattern))
            {
                if (this.adaptor.GetNodeType(t1) != t2.Type)
                {
                    return false;
                }
                if (t2.hasTextArg && !this.adaptor.GetNodeText(t1).Equals(t2.Text))
                {
                    return false;
                }
            }
            if ((t2.label != null) && (labels != null))
            {
                labels[t2.label] = t1;
            }
            int childCount = this.adaptor.GetChildCount(t1);
            int num2 = t2.ChildCount;
            if (childCount != num2)
            {
                return false;
            }
            for (int i = 0; i < childCount; i++)
            {
                object child = this.adaptor.GetChild(t1, i);
                TreePattern pattern = (TreePattern) t2.GetChild(i);
                if (!this._Parse(child, pattern, labels))
                {
                    return false;
                }
            }
            return true;
        }

        protected void _Visit(object t, object parent, int childIndex, int ttype, ContextVisitor visitor)
        {
            if (t != null)
            {
                if (this.adaptor.GetNodeType(t) == ttype)
                {
                    visitor.Visit(t, parent, childIndex, null);
                }
                int childCount = this.adaptor.GetChildCount(t);
                for (int i = 0; i < childCount; i++)
                {
                    object child = this.adaptor.GetChild(t, i);
                    this._Visit(child, t, i, ttype, visitor);
                }
            }
        }

        public IDictionary ComputeTokenTypes(string[] tokenNames)
        {
            IDictionary dictionary = new Hashtable();
            if (tokenNames != null)
            {
                for (int i = Token.MIN_TOKEN_TYPE; i < tokenNames.Length; i++)
                {
                    string key = tokenNames[i];
                    dictionary.Add(key, i);
                }
            }
            return dictionary;
        }

        public object Create(string pattern)
        {
            TreePatternLexer tokenizer = new TreePatternLexer(pattern);
            TreePatternParser parser = new TreePatternParser(tokenizer, this, this.adaptor);
            return parser.Pattern();
        }

        public bool Equals(object t1, object t2)
        {
            return _Equals(t1, t2, this.adaptor);
        }

        public static bool Equals(object t1, object t2, ITreeAdaptor adaptor)
        {
            return _Equals(t1, t2, adaptor);
        }

        public IList Find(object t, int ttype)
        {
            IList list = new List<object>();
            this.Visit(t, ttype, new RecordAllElementsVisitor(list));
            return list;
        }

        public IList Find(object t, string pattern)
        {
            IList list = new List<object>();
            TreePatternLexer tokenizer = new TreePatternLexer(pattern);
            TreePatternParser parser = new TreePatternParser(tokenizer, this, new TreePatternTreeAdaptor());
            TreePattern pattern2 = (TreePattern) parser.Pattern();
            if (((pattern2 == null) || pattern2.IsNil) || (pattern2.GetType() == typeof(WildcardTreePattern)))
            {
                return null;
            }
            int type = pattern2.Type;
            this.Visit(t, type, new PatternMatchingContextVisitor(this, pattern2, list));
            return list;
        }

        public object FindFirst(object t, int ttype)
        {
            return null;
        }

        public object FindFirst(object t, string pattern)
        {
            return null;
        }

        public int GetTokenType(string tokenName)
        {
            if (this.tokenNameToTypeMap != null)
            {
                object obj2 = this.tokenNameToTypeMap[tokenName];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
            }
            return 0;
        }

        public IDictionary Index(object t)
        {
            IDictionary m = new Hashtable();
            this._Index(t, m);
            return m;
        }

        public bool Parse(object t, string pattern)
        {
            return this.Parse(t, pattern, null);
        }

        public bool Parse(object t, string pattern, IDictionary labels)
        {
            TreePatternLexer tokenizer = new TreePatternLexer(pattern);
            TreePatternParser parser = new TreePatternParser(tokenizer, this, new TreePatternTreeAdaptor());
            TreePattern pattern2 = (TreePattern) parser.Pattern();
            return this._Parse(t, pattern2, labels);
        }

        public void Visit(object t, int ttype, ContextVisitor visitor)
        {
            this._Visit(t, null, 0, ttype, visitor);
        }

        public void Visit(object t, string pattern, ContextVisitor visitor)
        {
            TreePatternLexer tokenizer = new TreePatternLexer(pattern);
            TreePatternParser parser = new TreePatternParser(tokenizer, this, new TreePatternTreeAdaptor());
            TreePattern pattern2 = (TreePattern) parser.Pattern();
            if (((pattern2 != null) && !pattern2.IsNil) && (pattern2.GetType() != typeof(WildcardTreePattern)))
            {
                int type = pattern2.Type;
                this.Visit(t, type, new InvokeVisitorOnPatternMatchContextVisitor(this, pattern2, visitor));
            }
        }

        public interface ContextVisitor
        {
            void Visit(object t, object parent, int childIndex, IDictionary labels);
        }

        private sealed class InvokeVisitorOnPatternMatchContextVisitor : TreeWizard.ContextVisitor
        {
            private Hashtable labels = new Hashtable();
            private TreeWizard owner;
            private TreeWizard.TreePattern pattern;
            private TreeWizard.ContextVisitor visitor;

            public InvokeVisitorOnPatternMatchContextVisitor(TreeWizard owner, TreeWizard.TreePattern pattern, TreeWizard.ContextVisitor visitor)
            {
                this.owner = owner;
                this.pattern = pattern;
                this.visitor = visitor;
            }

            public void Visit(object t, object parent, int childIndex, IDictionary unusedlabels)
            {
                this.labels.Clear();
                if (this.owner._Parse(t, this.pattern, this.labels))
                {
                    this.visitor.Visit(t, parent, childIndex, this.labels);
                }
            }
        }

        private sealed class PatternMatchingContextVisitor : TreeWizard.ContextVisitor
        {
            private IList list;
            private TreeWizard owner;
            private TreeWizard.TreePattern pattern;

            public PatternMatchingContextVisitor(TreeWizard owner, TreeWizard.TreePattern pattern, IList list)
            {
                this.owner = owner;
                this.pattern = pattern;
                this.list = list;
            }

            public void Visit(object t, object parent, int childIndex, IDictionary labels)
            {
                if (this.owner._Parse(t, this.pattern, null))
                {
                    this.list.Add(t);
                }
            }
        }

        private sealed class RecordAllElementsVisitor : TreeWizard.Visitor
        {
            private IList list;

            public RecordAllElementsVisitor(IList list)
            {
                this.list = list;
            }

            public override void Visit(object t)
            {
                this.list.Add(t);
            }
        }

        public class TreePattern : CommonTree
        {
            public bool hasTextArg;
            public string label;

            public TreePattern(IToken payload) : base(payload)
            {
            }

            public override string ToString()
            {
                if (this.label != null)
                {
                    return ("%" + this.label + ":" + base.ToString());
                }
                return base.ToString();
            }
        }

        public class TreePatternTreeAdaptor : CommonTreeAdaptor
        {
            public override object Create(IToken payload)
            {
                return new TreeWizard.TreePattern(payload);
            }
        }

        public abstract class Visitor : TreeWizard.ContextVisitor
        {
            protected Visitor()
            {
            }

            public abstract void Visit(object t);
            public void Visit(object t, object parent, int childIndex, IDictionary labels)
            {
                this.Visit(t);
            }
        }

        public class WildcardTreePattern : TreeWizard.TreePattern
        {
            public WildcardTreePattern(IToken payload) : base(payload)
            {
            }
        }
    }
}

