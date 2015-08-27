using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

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

        public static Questionnaire CreateImportedQuestionnaire(Guid? creatorId = null, QuestionnaireDocument document = null)
        {
            var questionnaire = Create.DateCollectionQuestionnaire();

            questionnaire.ImportFromDesigner(new ImportFromDesigner(creatorId ?? new Guid(), document ?? new QuestionnaireDocument(), false, "base64 string of assembly"));

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaire()
        {
            return Create.DateCollectionQuestionnaire();
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