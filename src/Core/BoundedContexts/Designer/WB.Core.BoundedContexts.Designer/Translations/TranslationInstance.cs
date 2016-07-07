using System;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public class TranslationInstance
    {
        public virtual Guid QuestionnaireId { get; set; }

        public virtual TranslationType Type { get; set; }

        public virtual Guid QuestionnaireEntityId { get; set; }

        public virtual string TranslationIndex { get; set; }

        public virtual string Culture { get; set; }

        public virtual string Translation { get; set; }

        #region equals

        protected bool Equals(TranslationInstance other)
        {
            return this.QuestionnaireId.Equals(other.QuestionnaireId) && this.Type == other.Type &&
                   this.QuestionnaireEntityId.Equals(other.QuestionnaireEntityId) &&
                   this.TranslationIndex == other.TranslationIndex && string.Equals(this.Culture, other.Culture);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TranslationInstance) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.QuestionnaireId.GetHashCode();
                hashCode = (hashCode*397) ^ (int) this.Type;
                hashCode = (hashCode*397) ^ this.QuestionnaireEntityId.GetHashCode();
                hashCode = (hashCode*397) ^ (this.TranslationIndex?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (this.Culture?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        #endregion
    }
}