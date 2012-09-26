namespace Antlr.Runtime.Tree
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class RewriteRuleNodeStream : RewriteRuleElementStream<object>
    {
        public RewriteRuleNodeStream(ITreeAdaptor adaptor, string elementDescription) : base(adaptor, elementDescription)
        {
        }

        public RewriteRuleNodeStream(ITreeAdaptor adaptor, string elementDescription, IList<object> elements) : base(adaptor, elementDescription, elements)
        {
        }

        [Obsolete("This constructor is for internal use only and might be phased out soon. Use instead the one with IList<T>.")]
        public RewriteRuleNodeStream(ITreeAdaptor adaptor, string elementDescription, IList elements) : base(adaptor, elementDescription, elements)
        {
        }

        public RewriteRuleNodeStream(ITreeAdaptor adaptor, string elementDescription, object oneElement) : base(adaptor, elementDescription, oneElement)
        {
        }

        public object NextNode()
        {
            return base._Next();
        }

        protected override object ToTree(object el)
        {
            return base.adaptor.DupNode(el);
        }
    }
}

