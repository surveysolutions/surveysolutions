using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Tests.InterviewViewModelDenormalizerTests
{
    public class TextListQuestionViewModelTestContext
    {
        protected static TextListQuestionViewModel CreateTextListQuestionViewModel()
        {
            return new TextListQuestionViewModel(new InterviewItemId(Guid.NewGuid(), new decimal[0]), new ValueVector<Guid>(), "Text",
                QuestionType.TextList, true, "", "", true, false, null, "list", Enumerable.Empty<string>(), null, 8, new string[0]);
        }
    }
}
