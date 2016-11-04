using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using Group = Main.Core.Entities.SubEntities.Group;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Factories.QuestionnaireRosterStructureTests
{
    internal class QuestionnaireRosterStructureTestContext
    {
        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] children)
        {
            var result = new QuestionnaireDocument()
            {
                Children = new IComposite[]
                {
                    new Group("Chapter")
                    {
                        Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
                    }
                }.ToReadOnlyCollection()
            };

            return result;
        }

        protected static Mock<IQuestionnaireStorage> CreateQuestionnaireStorageMock(QuestionnaireDocument document, long version = 1, Guid? translationId = null)
        {
            var result = new Mock<IQuestionnaireStorage>();
            var questionnaire = new PlainQuestionnaire(document, version, translationId);
            result.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>()))
                .Returns(questionnaire);
            return result;
        }
    }
}
