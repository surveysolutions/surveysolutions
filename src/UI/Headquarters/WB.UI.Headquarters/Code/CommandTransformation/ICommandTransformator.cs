using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.UI.Headquarters.Code.CommandTransformation
{
    public interface ICommandTransformator
    {
        ICommand TransformCommnadIfNeeded(ICommand command, Guid? responsibleId = null);
        KeyValuePair<Guid, AbstractAnswer> ParseQuestionAnswer(UntypedQuestionAnswer answer,
            IQuestionnaire questionnaire);
    }
}