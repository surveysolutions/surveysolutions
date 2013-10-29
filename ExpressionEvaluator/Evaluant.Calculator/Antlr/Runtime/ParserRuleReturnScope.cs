namespace Antlr.Runtime
{
    using System;

    public class ParserRuleReturnScope : RuleReturnScope
    {
        private IToken start;
        private IToken stop;

        public override object Start
        {
            get
            {
                return this.start;
            }
            set
            {
                this.start = (IToken) value;
            }
        }

        public override object Stop
        {
            get
            {
                return this.stop;
            }
            set
            {
                this.stop = (IToken) value;
            }
        }
    }
}

