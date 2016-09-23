using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class GroupDeleted : QuestionnaireActive
    {
        /// <remarks>Needed for successfull deserialization from DB</remarks>>
        public GroupDeleted() {}

        public GroupDeleted(Guid groupPublicKey)
        {
            this.GroupPublicKey = groupPublicKey;
        }

        [Obsolete]
        public GroupDeleted(Guid groupPublicKey, Guid parentPublicKey)
            : this(groupPublicKey)
        {
            this.ParentPublicKey = parentPublicKey;
        }

        public Guid GroupPublicKey { get; set; }

        [Obsolete]
        public Guid ParentPublicKey { get; set; }
    }
}