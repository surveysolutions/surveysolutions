using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(SubstitionText))]
    [TestFixture]
    public class SubstitionTextTests
    {
        [Test]
        public void When_ReplaceSubstitutions_for_element_with_referancec_on_parent_rosters_Then_should_return_text_with_roster_titles()
        {
            //arrange
            var questionnireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var interviewId    = Guid.Parse("11111111111111111111111111111111");
            var rosterId1 = Guid.Parse("22222222222222222222222222222222");
            var rosterId2 = Guid.Parse("33333333333333333333333333333333");
            var questionId = Guid.Parse("44444444444444444444444444444444");
            var sectionIdentity = Create.Entity.Identity(Guid.Parse("55555555555555555555555555555555"));

            var questionnireDocument = Create.Entity.QuestionnaireDocument(questionnireId, new IComposite[]
            {
                Create.Entity.Roster(rosterId1, variable: "r1", children: new IComposite[]
                    {
                        Create.Entity.Roster(rosterId2, variable: "r2", children: new IComposite[]
                            {
                                Create.Entity.NumericQuestion(questionId, variableName: "n1")
                            })
                    })
            });
            var questionnire = Create.Entity.PlainQuestionnaire(questionnireDocument);

            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(sectionIdentity, children: new IInterviewTreeNode[]
            {
                Create.Entity.InterviewTreeRoster(Create.Entity.Identity(rosterId1, new decimal[] { 2 }), rosterTitle: "title 2", children: new IInterviewTreeNode[]
                {
                    Create.Entity.InterviewTreeRoster(Create.Entity.Identity(rosterId2, new decimal[] { 2, 1}), rosterTitle: "title 2.1", children: new IInterviewTreeNode[]
                    {
                        Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(questionId), questionType: QuestionType.Numeric, answer: 5),
                    }),
                }),
            });
            var tree = Create.Entity.InterviewTree(interviewId, sourceTreeMainSection);


            var substitionTextFactory = Create.Service.SubstitionTextFactory();
            var questionIdentity = Create.Entity.Identity(questionId, new RosterVector(new decimal[] {2, 1}));
            var substitionText = substitionTextFactory.CreateText(questionIdentity, "title: %r1% %r2%", questionnire);
            substitionText.SetTree(tree);

            //act
            substitionText.ReplaceSubstitutions();

            //assert
            Assert.That(substitionText.HasSubstitutions, Is.True);
            Assert.That(substitionText.Text, Is.EqualTo("title: title 2 title 2.1"));
        }
    }
}