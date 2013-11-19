using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDocumentUpgraderTests
{
    [Subject(typeof(QuestionnaireDocumentUpgrader))]
    internal class QuestionnaireDocumentUpgraderTestsContext
    {
        protected static QuestionnaireDocumentUpgrader CreateQuestionnaireDocumentUpgrader(IQuestionFactory questionFactory = null)
        {
            return new QuestionnaireDocumentUpgrader(
                questionFactory ?? Mock.Of<IQuestionFactory>(factory
                    => factory.CreateQuestion(it.IsAny<QuestionData>()) == Mock.Of<IQuestion>()));
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(params IComposite[] children)
        {
            return CreateQuestionnaireDocument(children.AsEnumerable());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(IEnumerable<IComposite> children = null)
        {
            var questionnaire = new QuestionnaireDocument();

            if (children != null)
            {
                questionnaire.Children.AddRange(children);
            }

            return questionnaire;
        }

        protected static Group CreateGroup(Guid? groupId = null, string title = "Group X",
            IEnumerable<IComposite> children = null, Action<Group> setup = null)
        {
            var group = new Group
            {
                PublicKey = groupId ?? Guid.NewGuid(),
                Title = title,
            };

            if (children != null)
            {
                group.Children.AddRange(children);
            }

            if (setup != null)
            {
                setup(group);
            }

            return group;
        }

        protected static AutoPropagateQuestion CreateAutoPropagatingQuestion(Guid? questionId = null,
            string title = "Question Title X", List<Guid> triggers = null)
        {
            return new AutoPropagateQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionText = title,
                QuestionType = QuestionType.AutoPropagate,
                Triggers = triggers ?? new List<Guid>(),
            };
        }
    }
}