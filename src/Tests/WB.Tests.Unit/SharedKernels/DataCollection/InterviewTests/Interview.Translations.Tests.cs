using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
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
        public void When_switch_translation_with_group_Then_title_has_substitutions_and_should_be_translated()
        {
            //arrange
            var ruTranslationName = "ru";
            var substSuffix = "%num%";
            var defaultSubst = "[...]";
            var ruTitle = "русский заголовок";

            var questionIdentity = Identity.Create(Guid.Parse("21111111111111111111111111111111"), RosterVector.Empty);
            var groupIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);

            var interview = Setup.StatefulInterviewWithMultilanguageQuestionnaires(new[]
            {
                new KeyValuePair<string, IComposite[]>(null, new IComposite[]
                {
                    Create.Entity.Group(groupIdentity.Id, "invariant title" + substSuffix),
                    Create.Entity.NumericIntegerQuestion(questionIdentity.Id, "num")
                }),
                new KeyValuePair<string, IComposite[]>(ruTranslationName, new IComposite[]
                {
                    Create.Entity.Group(groupIdentity.Id, ruTitle + substSuffix),
                    Create.Entity.NumericIntegerQuestion(questionIdentity.Id, "num")
                })
            });
            //act
            interview.SwitchTranslation(Create.Command.SwitchTranslation(ruTranslationName));
            //assert
            Assert.That(interview.GetTitleText(groupIdentity), Is.EqualTo(ruTitle + defaultSubst));
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
                Create.Entity.ValidationCondition(message: "ошибка 2"),
                Create.Entity.ValidationCondition(message: "предупреждение 1", severity: ValidationSeverity.Warning)
            });

            var staticTextIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);

            var interview = Setup.StatefulInterviewWithMultilanguageQuestionnaires(new[]
            {
                new KeyValuePair<string, IComposite[]>(null, new [] { 
                    Create.Entity.StaticText(staticTextIdentity.Id, "invariant text",
                        validationConditions: new List<ValidationCondition>(new []
            {
                Create.Entity.ValidationCondition(message: "error 1"),
                Create.Entity.ValidationCondition(message: "error 2"),
                Create.Entity.ValidationCondition(message: "warning 1", severity: ValidationSeverity.Warning)
            }))}),
                new KeyValuePair<string, IComposite[]>(ruTranslationName, new [] { Create.Entity.StaticText(staticTextIdentity.Id, ruText, validationConditions: ruValidationMessages) })
            });

            interview.Apply(Create.Event.StaticTextsDeclaredImplausible(staticTextIdentity, new[] {2}));
            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(new[] {0, 1 }, staticTextIdentity));

            //act
            interview.SwitchTranslation(Create.Command.SwitchTranslation(ruTranslationName));

            //assert
            Assert.That(interview.GetTitleText(staticTextIdentity), Is.EqualTo(ruText));
            Assert.That(interview.GetFailedValidationMessages(staticTextIdentity, "Error"),
                Is.EquivalentTo(new[] {$"{ruValidationMessages[0].Message} [1]", $"{ruValidationMessages[1].Message} [2]"}));

            Assert.That(interview.GetFailedWarningMessages(staticTextIdentity, "warning").Single(), Is.EqualTo($"{ruValidationMessages[2].Message} [3]"));
        }

        [Test]
        public void when_switch_translation_should_translate_substitutes_of_cascaded_values()
        {
            //arrange
            var ruTranslationName = "ru";

            var questionIdentity = Create.Entity.Identity(10);
            var cascadingIdentity = Create.Entity.Identity(20);
            var substituteIdentity = Create.Entity.Identity(30);

            var userId = Create.Entity.Identity(42).Id;
            
            var optionsRepo = Moq.Mock.Of<IQuestionOptionsRepository>(x =>
                x.GetOptionForQuestionByOptionValue(Moq.It.IsAny<IQuestionnaire>(), cascadingIdentity.Id, 2, 
                    Moq.It.IsAny<Translation>()) == new CategoricalOption
                {
                    Value = 2,
                    ParentValue = 2,
                    Title = "Опция значения 2"
                }
                && x.GetOptionsForQuestion(Moq.It.IsAny<IQuestionnaire>(), cascadingIdentity.Id, 2, "", Moq.It.IsAny<Translation>()) == new CategoricalOption
                {
                    Value = 2,
                    ParentValue = 2,
                    Title = "Опция значения 2"
                }.ToEnumerable()
            );

            var interview = Setup.StatefulInterviewWithMultilanguageQuestionnaires(
                new[] {

                    new KeyValuePair<string, IComposite[]>(null, new IComposite[]
                    {
                        Create.Entity.SingleOptionQuestion(questionIdentity.Id, "combobox",
                            answers: new List<Answer> {Create.Entity.Answer("EnValue1", 1), Create.Entity.Answer("EnvValue2", 2)}),
                        Create.Entity.SingleOptionQuestion(cascadingIdentity.Id, "cascading", cascadeFromQuestionId: questionIdentity.Id,
                            answers: new List<Answer> {Create.Entity.Answer("Default language %combobox% 1", 1, 1), Create.Entity.Answer("Default %combobox% value 2", 2, 2)}),
                        Create.Entity.TextQuestion(substituteIdentity.Id, "subst", text: "Sub me, and then, %combobox% и %cascading%")
                    }),

                    new KeyValuePair<string, IComposite[]>(ruTranslationName, new IComposite[]
                    {
                        Create.Entity.SingleOptionQuestion(questionIdentity.Id, "combobox",
                            answers: new List<Answer> {Create.Entity.Answer("Значение1", 1), Create.Entity.Answer("Значение2", 2)}),
                        Create.Entity.SingleOptionQuestion(cascadingIdentity.Id, "cascading", cascadeFromQuestionId: questionIdentity.Id,
                            answers: new List<Answer> {Create.Entity.Answer("Опция значения %combobox% 1", 1, 1), Create.Entity.Answer("Опция значения %combobox% 2", 2, 2)}),
                        Create.Entity.TextQuestion(substituteIdentity.Id, "subst", text: "Саб ми, %combobox% и %cascading%")
                    })

                },
                optionsRepo
            );

            Moq.Mock.Get(ServiceLocator.Current).Setup(_ => _.GetInstance<IQuestionOptionsRepository>()).Returns(optionsRepo);
            
            interview.AnswerSingleOptionQuestion(userId, questionIdentity.Id, questionIdentity.RosterVector, DateTime.UtcNow, 2); // значение 2
            interview.AnswerSingleOptionQuestion(userId, cascadingIdentity.Id, cascadingIdentity.RosterVector, DateTime.UtcNow, 2); // Опция значения 2

            //act
            interview.SwitchTranslation(Create.Command.SwitchTranslation(ruTranslationName));

            Assert.That(interview.GetTitleText(substituteIdentity), Is.EqualTo("Саб ми, Значение2 и Опция значения 2"));
        }


        [Test]
        public void When_switch_translation_with_static_text_having_substitutions_Then_title_and_validation_messages_should_be_translated()
        {
            //arrange
            var ruTranslationName = "ru";
            var ruText = "русский текст";
            var substSuffix = "%num%";
            var defaultSubst = "[...]";
            var ruValidationMessages = new List<ValidationCondition>(new[]
            {
                Create.Entity.ValidationCondition(message: "ошибка 1" + defaultSubst),
                Create.Entity.ValidationCondition(message: "ошибка 2" + defaultSubst)
            });

            var questionIdentity = Identity.Create(Guid.Parse("21111111111111111111111111111111"), RosterVector.Empty);
            var staticTextIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);

            var interview = Setup.StatefulInterviewWithMultilanguageQuestionnaires(new[]
            {
                new KeyValuePair<string, IComposite[]>(null, new IComposite[] 
                {
                    Create.Entity.NumericIntegerQuestion(questionIdentity.Id, "num"),
                    Create.Entity.StaticText(staticTextIdentity.Id, "invariant text" + substSuffix, 
                    validationConditions: new List<ValidationCondition>(new []
                    {
                        Create.Entity.ValidationCondition(message: "error 1" + substSuffix),
                        Create.Entity.ValidationCondition(message: "error 2" + substSuffix)
                    }))}),
                new KeyValuePair<string, IComposite[]>(ruTranslationName, new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(questionIdentity.Id, "num"),
                    Create.Entity.StaticText(staticTextIdentity.Id, ruText + substSuffix, 
                    validationConditions: new List<ValidationCondition>(new[]
                    {
                        Create.Entity.ValidationCondition(message: "ошибка 1" + substSuffix),
                        Create.Entity.ValidationCondition(message: "ошибка 2" + substSuffix)
                    }))})
            });
            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(new[] { 0, 1 }, staticTextIdentity));
            //act
            interview.SwitchTranslation(Create.Command.SwitchTranslation(ruTranslationName));
            //assert
            Assert.That(interview.GetTitleText(staticTextIdentity), Is.EqualTo(ruText + defaultSubst));
            Assert.That(interview.GetFailedValidationMessages(staticTextIdentity, "Error"),
                Is.EquivalentTo(new[] { $"{ruValidationMessages[0].Message} [1]", $"{ruValidationMessages[1].Message} [2]" }));
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
                Create.Entity.ValidationCondition(message: "ошибка 2"),
                Create.Entity.ValidationCondition(message: "предупреждение 1", severity: ValidationSeverity.Warning),
            });

            var questionIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);

            var interview = Setup.StatefulInterviewWithMultilanguageQuestionnaires(new[]
            {
                new KeyValuePair<string, IComposite[]>(null, new [] { Create.Entity.TextQuestion(questionIdentity.Id, text: "invariant title", validationConditions: new List<ValidationCondition>(new []
            {
                Create.Entity.ValidationCondition(message: "error 1"),
                Create.Entity.ValidationCondition(message: "error 2"),
                Create.Entity.ValidationCondition(message: "warning 2", severity: ValidationSeverity.Warning),

            }))}),
                new KeyValuePair<string, IComposite[]>(ruTranslationName, new [] { Create.Entity.TextQuestion(questionIdentity.Id, text: ruText, validationConditions: ruValidationMessages) })
            });

            interview.Apply(Create.Event.AnswersDeclaredImplausible(questionIdentity, new[] {2}));
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
            Assert.That(interview.GetFailedValidationMessages(questionIdentity, "Error"),
                Is.EquivalentTo(new[] {$"{ruValidationMessages[0].Message} [1]", $"{ruValidationMessages[1].Message} [2]"}));

            Assert.That(interview.GetFailedWarningMessages(questionIdentity, "warning").Single(), Is.EqualTo($"{ruValidationMessages[2].Message} [3]"));
        }

        [Test]
        public void When_switch_translation_with_question_having_substitutions_Then_title_and_validation_messages_should_be_translated()
        {
            //arrange
            var ruTranslationName = "ru";
            var ruText = "русский текст";
            var substSuffix = "%num%";
            var defaultSubst = "[...]";
            var ruValidationMessages = new List<ValidationCondition>(new[]
            {
                Create.Entity.ValidationCondition(message: "ошибка 1" + defaultSubst),
                Create.Entity.ValidationCondition(message: "ошибка 2" + defaultSubst)
            });

            var substQuestionIdentity = Identity.Create(Guid.Parse("21111111111111111111111111111111"), RosterVector.Empty);

            var questionIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);

            var interview = Setup.StatefulInterviewWithMultilanguageQuestionnaires(new[]
            {
                new KeyValuePair<string, IComposite[]>(null, new IComposite[] {
                    Create.Entity.NumericIntegerQuestion(substQuestionIdentity.Id, "num"),
                    Create.Entity.TextQuestion(questionIdentity.Id, text: "invariant title" + substSuffix, 
                    validationConditions: new List<ValidationCondition>(new []
                    {
                        Create.Entity.ValidationCondition(message: "error 1" + substSuffix),
                        Create.Entity.ValidationCondition(message: "error 2" + substSuffix)
                    }))}),
                new KeyValuePair<string, IComposite[]>(ruTranslationName, new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(substQuestionIdentity.Id, "num"),
                    Create.Entity.TextQuestion(questionIdentity.Id, text: ruText + substSuffix,
                    validationConditions: new List<ValidationCondition>(new[]
                    {
                        Create.Entity.ValidationCondition(message: "ошибка 1" + substSuffix),
                        Create.Entity.ValidationCondition(message: "ошибка 2" + substSuffix)
                    }))})
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
            Assert.That(interview.GetTitleText(questionIdentity), Is.EqualTo(ruText + defaultSubst));
            Assert.That(interview.GetFailedValidationMessages(questionIdentity, "Error"),
                Is.EquivalentTo(new[] { $"{ruValidationMessages[0].Message} [1]", $"{ruValidationMessages[1].Message} [2]" }));
        }
    }
}
