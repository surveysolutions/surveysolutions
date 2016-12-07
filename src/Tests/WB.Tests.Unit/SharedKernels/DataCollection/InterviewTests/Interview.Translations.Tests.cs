using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [Subject(typeof(Interview))]
    [TestOf(typeof(Interview))]
    public partial class InterviewTests
    {
        [Test]
        public void When_add_roster_after_switch_translation_Then_title_should_be_translated()
        {
            //arrange
            var ruTranslationName = "ru";
            var ruTitle = "русский заголовок";

            var rosterIdentity = Create.Entity.Identity("11111111111111111111111111111111", Create.Entity.RosterVector(0));

            var interview = Setup.StatefulInterviewWithMultilanguageQuestionnaires(new[]
            {
                new KeyValuePair<string, IComposite[]>(null, new [] { Create.Entity.Roster(rosterIdentity.Id, "invariant title")}),
                new KeyValuePair<string, IComposite[]>(ruTranslationName, new [] { Create.Entity.Roster(rosterIdentity.Id, ruTitle) })
            });

            //act
            interview.SwitchTranslation(Create.Command.SwitchTranslation(ruTranslationName));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterIdentity.Id, rosterIdentity.RosterVector));

            //assert
            Assert.That(interview.GetTitleText(rosterIdentity), Is.EqualTo(ruTitle));
        }

        [Test]
	    public void When_switch_translation_with_group_Then_title_should_be_translated()
	    {
	        //arrange
            var ruTranslationName = "ru";
            var ruTitle = "русский заголовок";

	        var groupIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);

		    var interview = Setup.StatefulInterviewWithMultilanguageQuestionnaires(new[]
		    {
				new KeyValuePair<string, IComposite[]>(null, new [] { Create.Entity.Group(groupIdentity.Id, "invariant title")}),
                new KeyValuePair<string, IComposite[]>(ruTranslationName, new [] { Create.Entity.Group(groupIdentity.Id, ruTitle) })
            });
	        //act
			interview.SwitchTranslation(Create.Command.SwitchTranslation(ruTranslationName));
	        //assert
	        Assert.That(interview.GetTitleText(groupIdentity), Is.EqualTo(ruTitle));
	    }

        [Test]
        public void When_switch_translation_with_static_text_Then_title_and_validation_messages_should_be_translated()
        {
            //arrange
            var ruTranslationName = "ru";
            var ruText = "русский текст";
            var ruValidationMessages = new List<ValidationCondition>(new []
            {
                Create.Entity.ValidationCondition(message: "ошибка 1"),
                Create.Entity.ValidationCondition(message: "ошибка 2")
            });

            var staticTextIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);

            var interview = Setup.StatefulInterviewWithMultilanguageQuestionnaires(new[]
            {
                new KeyValuePair<string, IComposite[]>(null, new [] { Create.Entity.StaticText(staticTextIdentity.Id, "invariant text", validationConditions: new List<ValidationCondition>(new []
            {
                Create.Entity.ValidationCondition(message: "error 1"),
                Create.Entity.ValidationCondition(message: "error 2")
            }))}),
                new KeyValuePair<string, IComposite[]>(ruTranslationName, new [] { Create.Entity.StaticText(staticTextIdentity.Id, ruText, validationConditions: ruValidationMessages) })
            });
            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(new[] {0, 1}, staticTextIdentity));
            //act
            interview.SwitchTranslation(Create.Command.SwitchTranslation(ruTranslationName));
            //assert
            Assert.That(interview.GetTitleText(staticTextIdentity), Is.EqualTo(ruText));
            Assert.That(interview.GetFailedValidationMessages(staticTextIdentity),
                Is.EquivalentTo(new[] {$"{ruValidationMessages[0].Message} [1]", $"{ruValidationMessages[1].Message} [2]"}));
        }

        [Test]
        public void When_switch_translation_with_question_Then_title_and_validation_messages_should_be_translated()
        {
            //arrange
            var ruTranslationName = "ru";
            var ruText = "русский текст";
            var ruValidationMessages = new List<ValidationCondition>(new[]
            {
                Create.Entity.ValidationCondition(message: "ошибка 1"),
                Create.Entity.ValidationCondition(message: "ошибка 2")
            });

            var questionIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);

            var interview = Setup.StatefulInterviewWithMultilanguageQuestionnaires(new[]
            {
                new KeyValuePair<string, IComposite[]>(null, new [] { Create.Entity.TextQuestion(questionIdentity.Id, text: "invariant title", validationConditions: new List<ValidationCondition>(new []
            {
                Create.Entity.ValidationCondition(message: "error 1"),
                Create.Entity.ValidationCondition(message: "error 2")
            }))}),
                new KeyValuePair<string, IComposite[]>(ruTranslationName, new [] { Create.Entity.TextQuestion(questionIdentity.Id, text: ruText, validationConditions: ruValidationMessages) })
            });
            interview.Apply(Create.Event.AnswersDeclaredInvalid(
                new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
                {
                    {
                        questionIdentity,
                        new List<FailedValidationCondition>(new[]
                        {
                            Create.Entity.FailedValidationCondition(0), Create.Entity.FailedValidationCondition(1)
                        })
                    }
                }));
            //act
            interview.SwitchTranslation(Create.Command.SwitchTranslation(ruTranslationName));
            //assert
            Assert.That(interview.GetTitleText(questionIdentity), Is.EqualTo(ruText));
            Assert.That(interview.GetFailedValidationMessages(questionIdentity),
                Is.EquivalentTo(new[] {$"{ruValidationMessages[0].Message} [1]", $"{ruValidationMessages[1].Message} [2]"}));
        }
    }
}
