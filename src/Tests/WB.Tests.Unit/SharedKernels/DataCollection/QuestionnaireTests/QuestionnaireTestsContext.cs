﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    [Subject(typeof(Questionnaire))]
    internal class QuestionnaireTestsContext
    {
        public static T GetSingleEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Single(e => e.Payload is T).Payload;
        }

        public static T GetLastEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Last(e => e.Payload is T).Payload;
        }

        public static Questionnaire CreateImportedQuestionnaire(Guid? creatorId = null, QuestionnaireDocument document = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null)
        {
            var questionnaire = Create.Other.DataCollectionQuestionnaire(plainQuestionnaireRepository: plainQuestionnaireRepository);

            questionnaire.ImportFromDesigner(new ImportFromDesigner(creatorId ?? new Guid(), document ?? new QuestionnaireDocument(), false, "base64 string of assembly", 1));

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaire()
        {
            return Create.Other.DataCollectionQuestionnaire();
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren.ToList(),
                    }
                }
            };
        }
    }
}