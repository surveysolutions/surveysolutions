using System;
using System.Collections.Generic;

using Machine.Specifications;

using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;

using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_interviewer_underlying_questions_in_group
    {
        Establish context = () =>
        {
            var questionnaire = Create.Other.QuestionnaireDocument(
                children: new List<IComposite>
                {
                    Create.Other.Group(groupId: chapterId, children: new List<IComposite>
                    {
                        Create.Other.NumericIntegerQuestion(Guid.NewGuid(), scope: QuestionScope.Headquarter),
                        Create.Other.NumericIntegerQuestion(Guid.NewGuid(), scope: QuestionScope.Supervisor),
                        Create.Other.NumericIntegerQuestion(Guid.NewGuid(), scope: QuestionScope.Interviewer, isPrefilled: true),
                        Create.Other.NumericIntegerQuestion(question1Id),
                        Create.Other.Roster(rosterSizeQuestionId: question1Id,
                                rosterTitleQuestionId: question2Id,
                                children: new List<IComposite>
                                {
                                    Create.Other.TextQuestion(questionId: question2Id),
                                    Create.Other.TextQuestion(questionId: question3Id),
                                    Create.Other.NumericIntegerQuestion(Guid.NewGuid(), scope: QuestionScope.Supervisor)
                                })
                        }),

                });

            plainQuestionnaire = Create.Other.PlainQuestionnaire(document: questionnaire);
        };

        Because of = () =>
            underlyingInterviewerQuestions = plainQuestionnaire.GetAllUnderlyingInterviewerQuestions(chapterId);

        It should_find_3_interviewer_questions = () => 
            underlyingInterviewerQuestions.ShouldContain(question1Id, question2Id, question2Id);

        private static PlainQuestionnaire plainQuestionnaire;
        private static readonly Guid question1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid question2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid question3Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid chapterId =     Guid.Parse("11111111111111111111111111111111");
        private static IEnumerable<Guid> underlyingInterviewerQuestions;
    }
}