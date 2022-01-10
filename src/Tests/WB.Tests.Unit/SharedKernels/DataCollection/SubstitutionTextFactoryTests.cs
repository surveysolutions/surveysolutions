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
        public void When_CreateText_with_reference_one_parent_rosters_Then_should_return_substition_text_with_substitions()
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
        
        [Test]
        public void When_CreateText_for_top_level_roster_with_rostertitle_reference_then_should_return_substition_variable_with_reference_on_self()
        {
            //arrange
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.Roster(Id.g1, title: "1st - %rostertitle%", children: new IComposite[]
                {
                    Create.Entity.Roster(Id.g2, title: "2nd - %rostertitle%", children: new IComposite[]
                    {
                        Create.Entity.Roster(Id.g3, title: "3d - %rostertitle%", children: new IComposite[]
                        {
                            Create.Entity.NumericQuestion(Id.g4)
                        })
                    })
                })
            });

            var questionnire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var substitionTextFactory = Create.Service.SubstitutionTextFactory();
            var rosterIdentity = Create.Entity.Identity(Id.g1, Create.Entity.RosterVector(1));

            //act
            var substitionText = substitionTextFactory.CreateText(rosterIdentity, "1st - %rostertitle%", questionnire);

            //assert
            Assert.That(substitionText.HasSubstitutions, Is.True);
            Assert.That(substitionText.substitutionVariables.Count, Is.EqualTo(1));
            Assert.That(substitionText.substitutionVariables[0].Id, Is.EqualTo(Id.g1));
            Assert.That(substitionText.substitutionVariables[0].Name, Is.EqualTo("rostertitle"));
        }
        
        [Test]
        public void When_CreateText_for_second_level_roster_with_rostertitle_reference_then_should_return_substition_variable_with_reference_on_self()
        {
            //arrange
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.Roster(Id.g1, title: "1st - %rostertitle%", children: new IComposite[]
                {
                    Create.Entity.Roster(Id.g2, title: "2nd - %rostertitle%", children: new IComposite[]
                    {
                        Create.Entity.Roster(Id.g3, title: "3d - %rostertitle%", children: new IComposite[]
                        {
                            Create.Entity.NumericQuestion(Id.g4)
                        })
                    })
                })
            });

            var questionnire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var substitionTextFactory = Create.Service.SubstitutionTextFactory();
            var rosterIdentity = Create.Entity.Identity(Id.g2, Create.Entity.RosterVector(1, 1));

            //act
            var substitionText = substitionTextFactory.CreateText(rosterIdentity, "2nd - %rostertitle%", questionnire);

            //assert
            Assert.That(substitionText.HasSubstitutions, Is.True);
            Assert.That(substitionText.substitutionVariables.Count, Is.EqualTo(1));
            Assert.That(substitionText.substitutionVariables[0].Id, Is.EqualTo(Id.g2));
            Assert.That(substitionText.substitutionVariables[0].Name, Is.EqualTo("rostertitle"));
        }

        [Test]
        public void When_CreateText_for_third_level_roster_with_rostertitle_reference_then_should_return_substition_variable_with_reference_on_self()
        {
            //arrange
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.Roster(Id.g1, title: "1st - %rostertitle%", children: new IComposite[]
                {
                    Create.Entity.Roster(Id.g2, title: "2nd - %rostertitle%", children: new IComposite[]
                    {
                        Create.Entity.Roster(Id.g3, title: "3d - %rostertitle%", children: new IComposite[]
                        {
                            Create.Entity.NumericQuestion(Id.g4)
                        })
                    })
                })
            });

            var questionnire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var substitionTextFactory = Create.Service.SubstitutionTextFactory();
            var rosterIdentity = Create.Entity.Identity(Id.g3, Create.Entity.RosterVector(1, 1, 1));

            //act
            var substitionText = substitionTextFactory.CreateText(rosterIdentity, "3d - %rostertitle%", questionnire);

            //assert
            Assert.That(substitionText.HasSubstitutions, Is.True);
            Assert.That(substitionText.substitutionVariables.Count, Is.EqualTo(1));
            Assert.That(substitionText.substitutionVariables[0].Id, Is.EqualTo(Id.g3));
            Assert.That(substitionText.substitutionVariables[0].Name, Is.EqualTo("rostertitle"));
        }

    }
}