using System;
using System.Collections.Generic;
using AutoMapper;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview.Services;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    public class WebInterviewInterviewEntityFactorySpecification
    {
        protected IQuestionnaire questionnaire;
        protected QuestionnaireDocument document;
        protected InterviewGroupOrRosterInstance RootGroupDetails;


        [OneTimeSetUp]
        public void Prepare()
        {
            this.document = GetDocument();
            this.questionnaire = Create.Entity.PlainQuestionnaire(this.document);
            

            var autoMapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebInterviewAutoMapProfile());
            });

            this.authorizedUserMock = new Mock<IAuthorizedUser>();

            Subject = new WebInterviewInterviewEntityFactory(autoMapperConfig.CreateMapper(), this.authorizedUserMock.Object);
        }

        [SetUp]
        public void Setup()
        {
            this.interview = Create.AggregateRoot.StatefulInterview(Guid.NewGuid(), questionnaire: this.document);
            Because();
            SetRootGroupDetails();
        }

        protected virtual void Because() { }

        protected virtual void SetRootGroupDetails() => this.RootGroupDetails = this.GetGroupDetails(SecA);
        
        protected virtual QuestionnaireDocument GetDocument()
        {
            return Create.Entity.QuestionnaireDocument(Guid.NewGuid(),

                Create.Entity.Group(SecA.Id, "Section A", "SecA", children: new IComposite[]
                {
                    Create.Entity.TextQuestion(SecA_In.Id, text: "Interviewer Question", variable: "text_in"),
                    Create.Entity.TextQuestion(SecA_Sup.Id, text: "Supervisor Questions", variable: "text_sup", scope: QuestionScope.Supervisor),

                    Create.Entity.FixedRoster(SecA_Roster.Id, title: "roster", children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(SecA_Roster_In.Id, text: "interviewer q in roster", variable: "text_in_r"),
                        Create.Entity.TextQuestion(SecA_Roster_Sup.Id, text: "supervisor q in roster", variable: "text_s_r", scope: QuestionScope.Supervisor),
                    }, fixedTitles: new [] { Create.Entity.FixedTitle(1, "Test") }),
                }),

                Create.Entity.Group(SecB.Id, "Section B", "SecB", children: new IComposite[]
                {
                    Create.Entity.TextQuestion(SecB_In.Id, text: "Interviewer Question", variable: "text_in_B"),
                    Create.Entity.TextQuestion(SecB_Sup.Id, text: "Supervisor Questions", variable: "text_sup_B", scope: QuestionScope.Supervisor),

                    Create.Entity.FixedRoster(SecB_Group.Id,variable: "roster_b", title: "roster", children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(SecB_Group_In.Id, text: "interviewer q in group", variable: "text_in_g_B"),
                        Create.Entity.TextQuestion(SecB_Group_Sup.Id, text: "supervisor q in group", variable: "text_s_g_B", scope: QuestionScope.Hidden),
                    }, fixedTitles: new [] { Create.Entity.FixedTitle(1, "Test") })
                }));
        }

        protected void AnswerTextQuestions(params Identity[] ids)
        {
            foreach (var id in ids)
            {
                this.interview.Apply(Create.Event.TextQuestionAnswered(id.Id, id.RosterVector, id.ToString()));
            }
        }

        protected void MarkQuestionAsInvalid(params Identity[] ids)
        {
            foreach (var identity in ids)
            {
                var failedConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>()
                {
                    {
                        identity,
                        new List<FailedValidationCondition> {new FailedValidationCondition(0)}
                    }
                };

                this.interview.Apply(Create.Event.AnswersDeclaredInvalid(failedConditions));
            }

        }

        protected Mock<IAuthorizedUser> authorizedUserMock;
        protected StatefulInterview interview;
        protected WebInterviewInterviewEntityFactory Subject { get; set; }

        protected static readonly Identity SecA = Id.IdentityA;
        protected static readonly Identity SecA_In = Id.Identity1;
        protected static readonly Identity SecA_Sup = Id.Identity2;

        protected static readonly Identity SecA_Roster = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAA11111111111111111"), 1);
        protected static readonly Identity SecA_Roster_In = Create.Identity(Id.g3, 1);
        protected static readonly Identity SecA_Roster_Sup = Create.Identity(Id.g4, 1);

        protected static readonly Identity SecB = Id.IdentityB;
        protected static readonly Identity SecB_In = Id.Identity5;
        protected static readonly Identity SecB_Sup = Id.Identity6;

        protected static readonly Identity SecB_Group = Create.Identity(Guid.Parse("BBBBBBBBBBBBBBB11111111111111111"), 1);
        protected static readonly Identity SecB_Group_In = Create.Identity(Id.g7, 1);
        protected static readonly Identity SecB_Group_Sup = Create.Identity(Id.g8, 1);

        
        protected InterviewGroupOrRosterInstance GetGroupDetails(Identity id)
        {
            var entity = Subject.GetEntityDetails(id.ToString(), this.interview, this.questionnaire, IsReviewMode);

            return entity as InterviewGroupOrRosterInstance;
        }

        protected virtual bool IsReviewMode { get; set; } = false;

        protected Identity[] AllGroups = { SecA, SecA_Roster, SecB, SecB_Group };
        

        protected void AsInterviewer()
        {
            IsReviewMode = false;
            this.authorizedUserMock.Setup(au => au.IsSupervisor).Returns(false);
        }

        protected void AsSupervisor()
        {
            IsReviewMode = true;
            this.authorizedUserMock.Setup(au => au.IsSupervisor).Returns(true);
        }
    }
}
