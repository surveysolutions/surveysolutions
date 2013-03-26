namespace Antlr.Runtime.Tree
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class RewriteRuleSubtreeStream : RewriteRuleElementStream<object>
    {
        public RewriteRuleSubtreeStream(ITreeAdaptor adaptor, string elementDescription) : base(adaptor, elementDescription)
        {
        }

        public RewriteRuleSubtreeStream(ITreeAdaptor adaptor, string elementDescription, IList<object> elements) : base(adaptor, elementDescription, elements)
        {
        }

        [Obsolete("This constructor is for internal use only and might be phased out soon. Use instead the one with IList<T>.")]
        public RewriteRuleSubtreeStream(ITreeAdaptor adaptor, string elementDescription, IList elements) : base(adaptor, elementDescription, elements)
        {
        }

        public RewriteRuleSubtreeStream(ITreeAdaptor adaptor, string elementDescription, object oneElement) : base(adaptor, elementDescription, oneElement)
        {
        }

        private object Dup(object el)
        {
            return base.adaptor.DupTree(el);
        }

        private object FetchObject(ProcessHandler ph)
        {
            if (this.RequiresDuplication())
            {
                return ph(base._Next());
            }
            return base._Next();
        }

        public object NextNode()
        {
            return this.FetchObject(o => base.adaptor.DupNode(o));
        }

        public override object NextTree()
        {
            return this.FetchObject(o => this.Dup(o));
        }

        private bool RequiresDuplication()
        {
            int count = base.Count;
            return (base.dirty || ((base.cursor >= count) && (count == 1)));
        }

        private delegate object ProcessHandler(object o);
    }
}

