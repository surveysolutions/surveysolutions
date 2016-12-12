using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using Group = Main.Core.Entities.SubEntities.Group;

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
            IQuestionnaireStorage questionnaireStorage = null)
        {
            var questionnaire = Create.AggregateRoot.Questionnaire(questionnaireStorage: questionnaireStorage);

            questionnaire.ImportFromDesigner(new ImportFromDesigner(creatorId ?? new Guid(), document ?? new QuestionnaireDocument(), false, "base64 string of assembly", 1 , 1));

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaire()
        {
            return Create.AggregateRoot.Questionnaire();
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
                        Children = chapterChildren.ToReadOnlyCollection(),
                    }
                }.ToReadOnlyCollection()
            };
        }
    }
}