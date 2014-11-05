using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.BoundedContexts.Supervisor;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom.Implementation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandTransformation;
using WB.UI.Designer.Api;

namespace WB.Tests.Unit
{
    internal static class Create
    {
        public static QuestionnaireDocument QuestionnaireDocument(Guid? id = null, params IComposite[] children)
        {
            return new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children != null ? children.ToList() : new List<IComposite>(),
            };
        }

        public static Group Chapter(string title = "Chapter X", IEnumerable<IComposite> children = null)
        {
            return Create.Group(
                title: title,
                children: children);
        }

        public static Group Group(Guid? id = null, string title = "Group X", string variable = null,
            string enablementCondition = null, IEnumerable<IComposite> children = null)
        {
            return new Group(title)
            {
                PublicKey = id ?? Guid.NewGuid(),
                VariableName = variable,
                ConditionExpression = enablementCondition,
                Children = children != null ? children.ToList() : new List<IComposite>(),
            };
        }

        public static IQuestion Question(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null, bool isMandatory = false)
        {
            return new TextQuestion("Question X")
            {
                PublicKey = id ?? Guid.NewGuid(),
                QuestionType = QuestionType.Text,
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Mandatory = isMandatory
            };
        }

        public static MultyOptionsQuestion MultyOptionsQuestion(Guid? id = null, bool isMandatory = false,
            IEnumerable<Answer> answers = null, Guid? linkedToQuestionId = null, string variable = null)
        {
            return new MultyOptionsQuestion
            {
                QuestionType = QuestionType.MultyOption,
                PublicKey = id ?? Guid.NewGuid(),
                Mandatory = isMandatory,
                Answers = linkedToQuestionId.HasValue ? null : new List<Answer>(answers ?? new Answer[] { }),
                LinkedToQuestionId = linkedToQuestionId,
                StataExportCaption = variable
            };
        }

        public static Group Roster(Guid? id = null, string title = "Roster X", string variable = null, string enablementCondition = null,
            string[] fixedTitles = null, IEnumerable<IComposite> children = null,
            RosterSizeSourceType rosterSizeSourceType = RosterSizeSourceType.FixedTitles,
            Guid? rosterSizeQuestionId = null, Guid? rosterTitleQuestionId = null)
        {
            Group group = Create.Group(
                id: id,
                title: title,
                variable: variable,
                enablementCondition: enablementCondition,
                children: children);

            group.IsRoster = true;
            group.RosterSizeSource = rosterSizeSourceType;
            if (rosterSizeSourceType == RosterSizeSourceType.FixedTitles)
                group.RosterFixedTitles = fixedTitles ?? new[] { "Roster X-1", "Roster X-2", "Roster X-3" };
            group.RosterSizeQuestionId = rosterSizeQuestionId;
            group.RosterTitleQuestionId = rosterTitleQuestionId;

            return group;
        }

        public static NumericQuestion NumericIntegerQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null, bool isMandatory = false)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = true,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Mandatory = isMandatory
            };
        }

        public static SingleQuestion SingleQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null, bool isMandatory = false,
            Guid? cascadeFromQuestionId = null, List<Answer> options = null)
        {
            return new SingleQuestion
            {
                QuestionType = QuestionType.SingleOption,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Mandatory = isMandatory,
                Answers = options ?? new List<Answer>(),
                CascadeFromQuestionId = cascadeFromQuestionId
            };
        }

        public static Answer Option(Guid? id = null, string text = null, string value = null, string parentValue = null)
        {
            return new Answer
            {
                PublicKey = id ?? Guid.NewGuid(),
                AnswerText = text ?? "text",
                AnswerValue = value ?? "1",
                ParentValue = parentValue
            };
        }

        private class SyncAsyncExecutorStub : IAsyncExecutor
        {
            public void ExecuteAsync(Action action)
            {
                action.Invoke();
            }
        }

        public static PdfQuestionnaireView PdfQuestionnaireView(Guid? publicId = null)
        {
            return new PdfQuestionnaireView
            {
                PublicId = publicId ?? Guid.Parse("FEDCBA98765432100123456789ABCDEF"),
            };
        }

        public static PdfQuestionView PdfQuestionView()
        {
            return new PdfQuestionView();
        }

        public static PdfGroupView PdfGroupView()
        {
            return new PdfGroupView();
        }

        public static RoslynExpressionProcessor RoslynExpressionProcessor()
        {
            return new RoslynExpressionProcessor();
        }

        public static CreateInterviewControllerCommand CreateInterviewControllerCommand()
        {
            return new CreateInterviewControllerCommand()
            {
                AnswersToFeaturedQuestions = new List<UntypedQuestionAnswer>()
            };
        }

        public static NCalcToCSharpConverter NCalcToCSharpConverter()
        {
            return new NCalcToCSharpConverter();
        }

        public static NCalcToSharpController NCalcToSharpController(ICommandService commandService = null,
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory = null)
        {
            return new NCalcToSharpController(
                Mock.Of<ILogger>(),
                commandService ?? Mock.Of<ICommandService>(),
                questionnaireInfoViewFactory ?? Mock.Of<IQuestionnaireInfoViewFactory>(),
                Create.SyncAsyncExecutor())
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration(),
            };
        }

        public static IAsyncExecutor SyncAsyncExecutor()
        {
            return new SyncAsyncExecutorStub();
        }

        public static NCalcToSharpController.OneQuestionnaireModel OneQuestionnaireModel(Guid id)
        {
            return new NCalcToSharpController.OneQuestionnaireModel { Id = id };
        }

        public static AtomFeedReader AtomFeedReader(Func<HttpMessageHandler> messageHandler = null, IHeadquartersSettings settings = null)
        {
            return new AtomFeedReader(
                messageHandler ?? Mock.Of<Func<HttpMessageHandler>>(),
                settings ?? Mock.Of<IHeadquartersSettings>());
        }
    }
}