﻿using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_length_of_selected_values_more_than_max_for_selected_answer_options_for_multiquestion_throw_interview_exception : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            var sectionId = Guid.Parse("1111111111111111111111111111111A");
            validatingQuestionId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: sectionId, children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: validatingQuestionId, answers: new [] { 1, 2, 3, 4 }, maxAllowedAnswers:2),
                
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
        };

        Because of = () => expectedException = Catch.Exception(() =>
            interview.AnswerMultipleOptionsQuestion(userId, validatingQuestionId, new decimal[] { }, DateTime.Now, new [] { 1, 2, 3 }));

        It should_raise_MultipleOptionsQuestionAnswered_event_with_QuestionId_equal_to_validatingQuestionId = () =>
            expectedException.ShouldBeOfExactType(typeof(AnswerNotAcceptedException));

        private static Exception expectedException;
        private static Guid validatingQuestionId;
        private static Interview interview;
        private static Guid userId;
    }
}
