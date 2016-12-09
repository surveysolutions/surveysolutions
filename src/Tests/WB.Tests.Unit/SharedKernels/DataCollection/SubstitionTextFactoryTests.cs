using System;
using System.Linq;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(SubstitionTextFactory))]
    [TestFixture]
    public class SubstitionTextFactoryTests
    {
        [Test]
        public void When_CreateText_with_referance_one_parent_rosteres_Then_should_return_substition_text_with_substitions()
        {
            //arrange
            var questionnireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var interviewId    = Guid.Parse("11111111111111111111111111111111");
            var rosterId1 = Guid.Parse("22222222222222222222222222222222");
            var rosterId2 = Guid.Parse("33333333333333333333333333333333");
            var questionId = Guid.Parse("44444444444444444444444444444444");

            var questionnireDocument = Create.Entity.QuestionnaireDocument(questionnireId, new IComposite[]
            {
                Create.Entity.Roster(rosterId1, variable: "r1", fixedRosterTitles: new FixedRosterTitle[]
                {
                    new FixedRosterTitle(1, "title 1"), 
                    new FixedRosterTitle(2, "title 2"), 
                }, 
                children: new IComposite[]
                    {
                        Create.Entity.Roster(rosterId2, variable: "r2", fixedRosterTitles: new FixedRosterTitle[]
                        {
                            new FixedRosterTitle(1, "title 1.1"),
                            new FixedRosterTitle(2, "title 2.1"),
                        },
                        children: new IComposite[]
                            {
                                Create.Entity.NumericQuestion(questionId, variableName: "n1", title: "title %r1% %r2%")
                            })
                    })
            });
            var questionnire = Create.Entity.PlainQuestionnaire(questionnireDocument);

            var substitionTextFactory = Create.Service.SubstitionTextFactory();
            var questionIdentity = Create.Entity.Identity(questionId, new RosterVector(new decimal[] {1, 1}));

            //act
            var substitionText = substitionTextFactory.CreateText(questionIdentity, "title %r1% %r2%", questionnire);

            //assert
            Assert.That(substitionText.HasSubstitutions, Is.True);
        }
    }
}