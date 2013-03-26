namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using System;

    public class TreeRuleReturnScope : RuleReturnScope
    {
        private object start;

        public override object Start
        {
            get
            {
                return this.start;
            }
            set
            {
                this.start = value;
            }
        }
    }
}

