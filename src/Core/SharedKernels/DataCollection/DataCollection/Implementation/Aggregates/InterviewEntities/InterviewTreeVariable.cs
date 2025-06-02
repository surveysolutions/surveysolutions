using System;
using System.Diagnostics;
using System.Net;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    [DebuggerDisplay("Variable {Identity}. Value = {Value}")]
    public class InterviewTreeVariable : InterviewTreeLeafNode
    {
        public object Value { get; private set; }
        public bool HasValue => this.Value != null;

        public InterviewTreeVariable(Identity identity) : base(identity)
        {
            this.Title = new SubstitutionText();
        }

        public override string ToString()
        {
            return $"Variable ({this.Identity}). Value = {Value ?? "'NULL'"}";
        }

        public void SetValue(object value)
        {
            if (value == null)
                this.Value = null;
            else
                this.Value = value switch
                {
                    string stringValue => stringValue.RemoveControlChars(),
                    DateTimeOffset dateTimeOffset => dateTimeOffset.DateTime,
                    _ => value
                };
        }

        public sealed override SubstitutionText Title { get; protected set; }

        public override IInterviewTreeNode Clone()
        {
            return (IInterviewTreeNode)this.MemberwiseClone();
        }

        public override void Accept(IInterviewTreeUpdater updater)
        {
            updater.UpdateEnablement(this);
            updater.UpdateVariable(this); 
        }

        public override NodeType NodeType => NodeType.Variable;

        public string GetValueAsString()
        {
            return !HasValue ? null : AnswerUtils.AnswerToString(Value, isTimestamp: false);
        }
        
        public string GetValueAsStringBrowserReady()
        {
            var valueAsString = GetValueAsString();
            return valueAsString == null? null : WebUtility.HtmlEncode(valueAsString);
        }
    }
}
