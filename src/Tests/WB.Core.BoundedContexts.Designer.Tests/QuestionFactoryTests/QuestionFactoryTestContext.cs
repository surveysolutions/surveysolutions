using System;
using System.Collections.Generic;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionFactoryTests
{
    internal class QuestionFactoryTestContext
    {
        public static QuestionFactory CreateQuestionFactory()
        {
            return new QuestionFactory();
        }

        public static QuestionData CreateQuestionData(QuestionType QuestionType)
        {
            return new QuestionData(Guid.NewGuid(), QuestionType, QuestionScope.Interviewer, "title", "var", "", "", "",
                Order.AsIs, false, false, false, "", new List<Guid>(), null, new Answer[0], null, null, null, null, null);
        }
    }
}