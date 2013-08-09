using System;
using System.Collections.Generic;

namespace Main.Core.AbstractFactories
{
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    public interface ICompleteQuestionFactory
    {
        ICompleteQuestion ConvertToCompleteQuestion(IQuestion question);

        AbstractQuestion CreateQuestion(Guid publicKey, QuestionType questionType, QuestionScope questionScope, string questionText, string stataExportCaption, string conditionExpression, string validationExpression, string validationMessage, Order answerOrder, bool featured, bool mandatory, bool capital, string instructions, List<Guid> triggers, int maxValue, Answer[] answers);

        IQuestion CreateQuestionFromExistingUsingSpecifiedData(IQuestion question, QuestionType questionType, QuestionScope questionScope, string questionText, string stataExportCaption, string conditionExpression, string validationExpression, string validationMessage, Order answerOrder, bool featured, bool mandatory, bool capital, string instructions, List<Guid> triggers, int maxValue, Answer[] answers);
    }
}