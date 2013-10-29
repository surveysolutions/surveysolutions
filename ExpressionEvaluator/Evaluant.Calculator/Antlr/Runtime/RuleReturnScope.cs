namespace Antlr.Runtime
{
    using System;

    public class RuleReturnScope
    {
        public virtual object Start
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotSupportedException("Setter has not been defined for this property.");
            }
        }

        public virtual object Stop
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotSupportedException("Setter has not been defined for this property.");
            }
        }

        public virtual object Template
        {
            get
            {
                return null;
            }
        }

        public virtual object Tree
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotSupportedException("Setter has not been defined for this property.");
            }
        }
    }
}

