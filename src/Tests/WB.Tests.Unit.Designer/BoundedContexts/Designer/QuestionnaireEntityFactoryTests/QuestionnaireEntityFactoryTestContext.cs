using System;
using System.Collections.Generic;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireEntityFactoryTests
{
    internal class QuestionnaireEntityFactoryTestContext
    {
        protected static QuestionnaireEntityFactory CreateFactory()
        {
            return new QuestionnaireEntityFactory();
        }

        protected static QuestionData CreateQuestionData(QuestionType questionType, Guid? questionId = null, string title = null,
            string variable = null, string instructions = null, string condition = null)
        {
            return new QuestionData(
                questionId ?? Guid.NewGuid(), 
                questionType, 
                QuestionScope.Interviewer, 
                title ?? "title", 
                variable ?? "var", 
                null,
                condition ?? "", 
                false,
                Order.AsIs,
                false,
                false,
                instructions ?? "", null,
                new Answer[0], 
                null,
                null,
                null, 
                null, 
                null, 
                null, 
                null,
                null,
                null,
                null,
                new List<ValidationCondition>(),
                null);
        }
    }
}