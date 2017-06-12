using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_referencing_questionnaire_level_roster_in_validation : in_standalone_app_domain
    {
        private Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                

                var options = new List<Answer> {Create.Entity.Option(1), Create.Entity.Option(2)};
                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.TextListQuestion(listQuestionId, variable: "list"),
                    Create.Entity.ListRoster(rosterId, variable: "members", rosterSizeQuestionId: listQuestionId, children: new IComposite[]
                    {
                        Create.Entity.SingleQuestion(statusQuestionId, variable: "status", options: options, validationExpression: "(status!=1) || (status==1 && (members.Count(x=>x.status==1)==1))")
                    })
                );

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.Now, Create.Entity.ListAnswer(2,4,5).ToTupleArray());
                interview.AnswerSingleOptionQuestion(userId, statusQuestionId, Create.RosterVector(2), DateTime.Now, 1);
                
                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(userId, statusQuestionId, Create.RosterVector(4), DateTime.Now, 1);

                    return new InvokeResults
                    {
                        Status1DeclaredInvalid = eventContext.AnyEvent<AnswersDeclaredInvalid>(e => e.Questions.Contains(Create.Identity(statusQuestionId, Create.RosterVector(2)))),
                        Status2DeclaredInvalid = eventContext.AnyEvent<AnswersDeclaredInvalid>(e => e.Questions.Contains(Create.Identity(statusQuestionId, Create.RosterVector(4))))
                    };
                }
            });

        It should_declare_status_for_person1_as_invalid = () =>
            results.Status1DeclaredInvalid.ShouldBeTrue();

        It should_declare_status_for_person2_as_invalid = () =>
            results.Status2DeclaredInvalid.ShouldBeTrue();

      
        private static InvokeResults results;
        static readonly Guid questionnaireId = Guid.Parse("77778888000000000000000000000000");
        static readonly Guid statusQuestionId = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid listQuestionId = Guid.Parse("33333333333333333333333333333333");
        static readonly Guid rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

        [Serializable]
        internal class InvokeResults
        {
            public bool Status2DeclaredInvalid { get; set; }
            public bool Status1DeclaredInvalid { get; set; }
        }
    }
}