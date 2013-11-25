using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireTests
{
    internal class when_questionnaire_have_autopropagate_question_with_not_existing_group_in_triggers_then_GetRosterGroupsByRosterSizeQuestion_should_throw_exception
        : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var rosterSizeQuestionId = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterGroupId = new Guid("EBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            IQuestionnaireDocument questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                new NumericQuestion() { PublicKey = rosterSizeQuestionId, IsInteger = true, MaxValue = 4 },
                new AutoPropagateQuestion()
                {
                    PublicKey = autopropagateQuestionId,
                    QuestionType = QuestionType.AutoPropagate,
                    Triggers = new List<Guid>() { Guid.NewGuid() }
                },
                new Group() { PublicKey = autopropagateGroupId, Propagated = Propagate.AutoPropagated },
                new Group() { PublicKey = rosterGroupId, IsRoster = true, RosterSizeQuestionId = rosterSizeQuestionId },
            });

            questionnaire = CreateQuestionnaire(Guid.NewGuid(), questionnaireDocument);
        };


        Because of = () =>
            exception = Catch.Exception(() => 
                questionnaire.GetRosterGroupsByRosterSizeQuestion(autopropagateQuestionId));


        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__propagating__ = () =>
            exception.Message.ToLower().ShouldContain("propagating");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        It should_throw_exception_with_message_containting__references__ = () =>
            exception.Message.ToLower().ShouldContain("references");

        It should_throw_exception_with_message_containting__group__ = () =>
            exception.Message.ToLower().ShouldContain("group");

        It should_throw_exception_with_message_containting__missing__ = () =>
            exception.Message.ToLower().ShouldContain("missing");

        It should_throw_exception_with_message_containting_autopropagate_question_id = () =>
            exception.Message.ShouldContain(autopropagateQuestionId.ToString("N"));

        private static Questionnaire questionnaire;
        private static Guid autopropagateQuestionId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid autopropagateGroupId = new Guid("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Exception exception;
    }
}