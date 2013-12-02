using System;
using System.Collections.Generic;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionFactoryTests
{
    internal class QuestionFactoryTestContext
    {
        protected static QuestionFactory CreateQuestionFactory()
        {
            return new QuestionFactory();
        }

        protected static QuestionData CreateQuestionData(QuestionType questionType)
        {
            return new QuestionData(Guid.NewGuid(), questionType, QuestionScope.Interviewer, "title", "var", "", "", "",
                Order.AsIs, false, false, false, "", new List<Guid>(), null, new Answer[0], null, null, null, null, null);
        }
    }
}