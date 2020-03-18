using System;
using System.Collections.Generic;
using AutoMapper;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.Controllers.Api.PublicApi;
using It = Moq.It;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public abstract class AssignmentsPublicApiMapProfileSpecification
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var mapConfig = new MapperConfiguration(c =>
            {
                c.AddProfile<AssignmentsPublicApiMapProfile>();
                c.ConstructServicesUsing(t => this.ServiceMock[t]);
            });

            mapper = mapConfig.CreateMapper();

            Context();

            this.storageMock = new Mock<IQuestionnaireStorage>();
            storageMock.Setup(s => s.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns(Questionnaire);
            ServiceMock.Add(typeof(IQuestionnaireStorage), this.storageMock.Object);
            Because();
        }

        public abstract void Context();

        public abstract void Because();

        protected Mock<IQuestionnaireStorage> storageMock;
        protected Dictionary<Type, object> ServiceMock = new Dictionary<Type, object>();

        protected IMapper mapper;

        protected IQuestionnaire Questionnaire { get; set; } = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Id.g1,
            children: new[]
            {
                Create.Entity.TextQuestion(questionId: Id.g2, variable: "test2", preFilled: true),
                Create.Entity.TextQuestion(questionId: Id.g3, variable: "test3", preFilled: true),
                Create.Entity.TextQuestion(questionId: Id.g4, variable: "test4", preFilled: true)
            }));
    }
}
