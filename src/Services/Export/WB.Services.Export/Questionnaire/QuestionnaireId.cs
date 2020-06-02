using System;
using System.Diagnostics;

namespace WB.Services.Export.Questionnaire
{
    [DebuggerStepThrough]
    public class QuestionnaireId
    {
        public QuestionnaireId(string id)
        {
            this.Id = id;
        }
        
        protected bool Equals(QuestionnaireId other)
        {
            return string.Equals(Id, other.Id, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QuestionnaireId) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public string Id { get; protected set; }

        public override string ToString()
        {
            return Id;
        }
    }
}
