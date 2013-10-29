namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class RewriteRuleTokenStream : RewriteRuleElementStream<IToken>
    {
        public RewriteRuleTokenStream(ITreeAdaptor adaptor, string elementDescription) : base(adaptor, elementDescription)
        {
        }

        public RewriteRuleTokenStream(ITreeAdaptor adaptor, string elementDescription, IToken oneElement) : base(adaptor, elementDescription, oneElement)
        {
        }

        public RewriteRuleTokenStream(ITreeAdaptor adaptor, string elementDescription, IList<IToken> elements) : base(adaptor, elementDescription, elements)
        {
        }

        [Obsolete("This constructor is for internal use only and might be phased out soon. Use instead the one with IList<T>.")]
        public RewriteRuleTokenStream(ITreeAdaptor adaptor, string elementDescription, IList elements) : base(adaptor, elementDescription, elements)
        {
        }

        public object NextNode()
        {
            return base.adaptor.Create((IToken) base._Next());
        }

        public IToken NextToken()
        {
            return (IToken) base._Next();
        }

        protected override object ToTree(IToken el)
        {
            return el;
        }
    }
}

