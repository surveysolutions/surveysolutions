using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    class when_answer_on_roster_title_question_is_changed_and_roster_title_is_used_in_a_condition:InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            isQuestionWithRosterTitleConditionEnabled = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var questionToEnableId = Guid.Parse("11111111111111111111111111111111");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var rosterQuestionId = Guid.Parse("22222222222222222222222222222222");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                const string enablementCondition = "@rowname==\"yes\"";

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(rosterSizeQuestionId, variable: "a"),
                    Create.Roster(rosterId, rosterSizeSourceType: RosterSizeSourceType.Question, rosterTitleQuestionId:rosterQuestionId,
                        rosterSizeQuestionId: rosterSizeQuestionId, children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(questionToEnableId,enablementCondition: enablementCondition),
                            Create.TextQuestion(rosterQuestionId)
                        })
                    );

                var interview = SetupInterview(questionnaireDocument);
                
                interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, 1);
                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextQuestion(userId, rosterQuestionId, new decimal[]{0}, DateTime.Now, "yes");

                    return eventContext.AnyEvent<QuestionsEnabled>(
                        where =>
                            where.Questions.Count(
                                instance =>
                                    instance.Id == questionToEnableId &&
                                    instance.RosterVector.SequenceEqual(new decimal[] {0})) == 1);
                }
            });

        It should_enable_question_with_condition_which_uses_roster_title = () =>
            isQuestionWithRosterTitleConditionEnabled.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static bool isQuestionWithRosterTitleConditionEnabled;
        private static AppDomainContext appDomainContext;
    }
}
