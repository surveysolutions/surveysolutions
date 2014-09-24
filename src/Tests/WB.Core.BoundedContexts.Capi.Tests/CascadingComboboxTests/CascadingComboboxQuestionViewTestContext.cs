using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Tests.CascadingComboboxTests
{
    internal class CascadingComboboxQuestionViewTestContext
    {
        protected static CascadingComboboxQuestionViewModel CreateCascadingComboboxQuestionViewModel(Func<decimal[], IEnumerable<AnswerViewModel>> getAnswerOptions)
        {
            return new CascadingComboboxQuestionViewModel(
                new InterviewItemId(Guid.NewGuid()),
                new ValueVector<Guid>(), 
                "Hello",
                getAnswerOptions,
                true,
                "Instructions",
                null,
                true,
                false,
                null,
                "Validation Message",
                "variable",
                new string[0]);
        }
    }
}
