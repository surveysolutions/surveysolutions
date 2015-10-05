using System;
using System.Linq.Expressions;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public struct QuestionnaireIdentity
    {
        public QuestionnaireIdentity(Guid questionnaireId, long version)
        {
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
        }

        public long Version;

        public Guid QuestionnaireId;

        public bool Equals(QuestionnaireIdentity other)
        {
            return this.Version == other.Version && this.QuestionnaireId.Equals(other.QuestionnaireId);
        }

        public override string ToString()
        {
            return string.Format("{0}${1}", this.QuestionnaireId.FormatGuid(), this.Version);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Version.GetHashCode() * 863) ^ this.QuestionnaireId.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is QuestionnaireIdentity && this.Equals((QuestionnaireIdentity)obj);
        }

        public static QuestionnaireIdentity Parse(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("the id is null or empty string");

            var idParameters = id.Split('$');
            if (idParameters.Length != 2)
                throw new FormatException(String.Format("id value '{0}' is not in the correct format.", id));

            try
            {
                var questionnaireId = Guid.Parse(idParameters[0]);
                var version = long.Parse(idParameters[1]);

                return new QuestionnaireIdentity(questionnaireId, version);
            }
            catch (Exception e)
            {
                throw new FormatException(String.Format("id value '{0}' is not in the correct format.", id), e);
            }
        }
    }
}
