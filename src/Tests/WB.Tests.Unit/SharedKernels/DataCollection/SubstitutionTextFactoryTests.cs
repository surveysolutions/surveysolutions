using System;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(SubstitutionTextFactory))]
    [TestFixture]
    public class SubstitutionTextFactoryTests
    {
        [Test]
        public void When_CreateText_with_referance_one_parent_rosteres_Then_should_return_substition_text_with_substitions()
        {
            //arrange
            var questionId = Guid.Parse("44444444444444444444444444444444");

            var questionnireDocument = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.Roster(variable: "r1", children: new IComposite[]
                {
                    Create.Entity.Roster(variable: "r2", children: new IComposite[]
                    {
                        Create.Entity.NumericQuestion(questionId)
                    })
                })
            });

            var questionnire = Create.Entity.PlainQuestionnaire(questionnireDocument);
            var substitionTextFactory = Create.Service.SubstitutionTextFactory();
            var questionIdentity = Create.Entity.Identity(questionId, Create.Entity.RosterVector(1, 1));

            //act
            var substitionText = substitionTextFactory.CreateText(questionIdentity, "title %r1% %r2%", questionnire);

            //assert
            Assert.That(substitionText.HasSubstitutions, Is.True);
        }
    }
}