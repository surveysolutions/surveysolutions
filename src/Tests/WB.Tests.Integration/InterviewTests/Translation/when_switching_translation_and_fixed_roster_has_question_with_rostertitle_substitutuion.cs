﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.Translation
{
    internal class when_switching_translation_and_fixed_roster_has_question_with_rostertitle_substitutuion : InterviewTestsContext
    {
        private Establish context = () =>
        {
            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: QuestionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.Roster(rosterId,
                        variable: "varRoster",
                        rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                        fixedTitles: new[] { "a", "b" },
                        children: new List<IComposite>
                        {
                            Create.Entity.TextQuestion(questionId: t1Id, variable: "test",
                                text: "title with %rostertitle%")
                        })
                    
            });

            QuestionnaireDocument questionnaireWithTranslation = questionnaire.Clone();
            questionnaireWithTranslation.Find<Group>(x => x.IsRoster).First().FixedRosterTitles[1].Title = "test";

            var doc = IntegrationCreate.PlainQuestionnaire(questionnaire);
            var docWithTranslation = IntegrationCreate.PlainQuestionnaire(questionnaireWithTranslation);

            var repo = Mock.Of<IQuestionnaireStorage>(repository
                =>
                    repository.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), "testTranslation") == docWithTranslation &&
                    repository.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), null) == doc
                    && repository.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire
                    && repository.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) == questionnaire);

            questionnaire.Translations.Add(new Core.SharedKernels.SurveySolutions.Documents.Translation {Name = translationName , Id = translationId});

            interview = SetupStatefullInterview(questionnaire, questionnaireStorage: repo);
           
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.SwitchTranslation(new SwitchTranslation(interview.Id, "testTranslation", userId));

        It should_raise_VariablesValuesChanged_event_for_the_variable = () =>
            interview.GetTitleText(Create.Identity(t1Id, 1)).ShouldEqual("title with test");

        static EventContext eventContext;
        static StatefulInterview interview;
        static readonly Guid QuestionnaireId = Guid.Parse("10000000000000000000000000000000");
        static readonly Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        static readonly Guid t1Id = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid rosterId =  Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        static readonly Guid translationId = Guid.Parse("AFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        static readonly string translationName = "testTranslation";
    }
}