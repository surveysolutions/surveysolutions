﻿using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionerTests
{
    internal class QuestionnaireVersionerTestContext
    {
        protected static QuestionnaireVersioner CreateQuestionnaireVersioner()
        {
            return new QuestionnaireVersioner();
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument()
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new TextQuestion() {QuestionType = QuestionType.Text}
                }
            }; 
        }
    }
}
