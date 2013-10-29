namespace Antlr.Runtime.Tree
{
    using System;

    public class TreeVisitor
    {
        protected ITreeAdaptor adaptor;

        public TreeVisitor() : this(new CommonTreeAdaptor())
        {
        }

        public TreeVisitor(ITreeAdaptor adaptor)
        {
            this.adaptor = adaptor;
        }

        public object Visit(object t, ITreeVisitorAction action)
        {
            bool flag = this.adaptor.IsNil(t);
            if ((action != null) && !flag)
            {
                t = action.Pre(t);
            }
            int childCount = this.adaptor.GetChildCount(t);
            for (int i = 0; i < childCount; i++)
            {
                object child = this.adaptor.GetChild(t, i);
                object obj3 = this.Visit(child, action);
                object obj4 = this.adaptor.GetChild(t, i);
                if (obj3 != obj4)
                {
                    this.adaptor.SetChild(t, i, obj3);
                }
            }
            if ((action != null) && !flag)
            {
                t = action.Post(t);
            }
            return t;
        }
    }
}

