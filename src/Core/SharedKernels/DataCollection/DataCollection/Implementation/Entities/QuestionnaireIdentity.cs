using System;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public sealed class QuestionnaireIdentity
    {
        public QuestionnaireIdentity() { }
        public QuestionnaireIdentity(Guid questionnaireId, long version)
        {
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.Id = this.ToString();
        }

        public long Version { get; set; }
        public Guid QuestionnaireId { get; set; }
        public string Id { get; set; }

        public bool Equals(QuestionnaireIdentity other)
        {
            return this.Version == other.Version && this.QuestionnaireId.Equals(other.QuestionnaireId);
        }

        public override string ToString()
        {
            return this.QuestionnaireId.FormatGuid()  + "$" + this.Version;
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

        public static bool TryParse(string questionnaireIdentity, out QuestionnaireIdentity result)
        {
            try
            {
                result = Parse(questionnaireIdentity);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static QuestionnaireIdentity Parse(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id), "the id is null or empty string");

            var idParameters = id.Split('$');
            if (idParameters.Length != 2)
                throw new FormatException($"id value '{id}' is not in the correct format.");

            try
            {
                var questionnaireId = Guid.Parse(idParameters[0]);
                var version = long.Parse(idParameters[1]);

                return new QuestionnaireIdentity(questionnaireId, version);
            }
            catch (Exception e)
            {
                throw new FormatException($"id value '{id}' is not in the correct format.", e);
            }
        }
    }
}
