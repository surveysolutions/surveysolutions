using System;
using System.Linq;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    [TestFixture]
    public class GetSubSectionsWithEnablementConditionTests
    {
        [Test]
        public void when_GetSubSectionsWithEnablementCondition()
        {
            var parentGroup = Guid.NewGuid();
            var groupWithEnablement = Guid.NewGuid();
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.Group(parentGroup, children: new IComposite[]
                {
                    Create.Entity.StaticText(),
                    Create.Entity.TextQuestion(),
                    Create.Entity.Group(children: new[]
                    {
                        Create.Entity.StaticText()
                    }),
                    Create.Entity.Group(enablementCondition:"condition", groupId: groupWithEnablement, children: new[]
                    {
                        Create.Entity.StaticText()
                    }),
                    Create.Entity.Roster(children: new[]
                    {
                        Create.Entity.StaticText()
                    })
                })
            });
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 0);


            var subsections = plainQuestionnaire.GetSubSectionsWithEnablementCondition(parentGroup);

            Assert.AreEqual(subsections.Count, 1);
            Assert.AreEqual(subsections.Single(), groupWithEnablement);
        }
    }
}
