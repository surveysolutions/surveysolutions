namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PropagatedGridViewInputModel
    {
        public PropagatedGridViewInputModel(Guid compelteQuestionnairePublicKey, Guid groupPublicKey)
        {
            CompelteQuestionnairePublicKey = compelteQuestionnairePublicKey;
            GroupPublicKey = groupPublicKey;
        }

        public Guid CompelteQuestionnairePublicKey { get; set; }
        public Guid GroupPublicKey { get; set; }
    }
}
